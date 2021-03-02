﻿module BusWire

open Fable.React
open Fable.React.Props
open Browser
open Elmish
open Elmish.React
open Helpers


//------------------------------------------------------------------------//
//------------------------------BusWire Types-----------------------------//
//------------------------------------------------------------------------//


/// type for buswires
/// for demo only. The real wires will
/// connect to Ports - not symbols, where each symbol has
/// a number of ports (see Issie Component and Port types) and have
/// extra information for highlighting, width, etc.
/// NB - how you define Ports for drawing - whether they correspond to
/// a separate datatype and Id, or whether port offsets from
/// component coordinates are held in some other way, is up to groups.

type WireSegment = {
    wId: CommonTypes.ConnectionId
    Id: CommonTypes.ConnectionId 
    SrcPos: XYPos
    TargetPos: XYPos
    BB: XYPos * XYPos
    Highlight: bool
    }
type Wire = {
    Id: CommonTypes.ConnectionId 
    SrcPort: CommonTypes.PortId
    TargetPort: CommonTypes.PortId
    Segments: WireSegment list
    WireHighlight: bool
    }

type Model = {
    Symbol: Symbol.Model
    WX: Wire list
    Color: CommonTypes.HighLightColor
    }

let makeWireBB (sPos:XYPos) (tPos:XYPos)=
    match sPos.X,sPos.Y, tPos.X, tPos.Y with
    | (sx,sy,tx,ty) when tx>sx -> ({X=sx;Y=sy-5.},{X=tx;Y=ty+5.})
    | (sx,sy,tx,ty) when tx<sx -> ({X=tx;Y=ty-5.},{X=sx;Y=sy+5.})
    | (sx,sy,tx,ty) when ty>sy -> ({X=sx-5.;Y=sy},{X=tx+5.;Y=ty})
    | (sx,sy,tx,ty) when ty<sy -> ({X=tx-5.;Y=ty},{X=tx+5.;Y=sy})
    | (sx,sy,tx,ty) when (tx=sx && ty=sy) -> ({X=tx;Y=ty},{X=sx;Y=sy})
    | _ -> failwithf "diagonal line error"

let makeWireSegment (WId:CommonTypes.ConnectionId) (sPos) (tPos) = 
    //printf "%A" (makeWireBB sPos tPos)
    {
        wId=WId
        Id=CommonTypes.ConnectionId (uuid())
        SrcPos=sPos
        TargetPos=tPos
        BB= makeWireBB sPos tPos
        Highlight=false
    }
let bbCollision (bb1:XYPos*XYPos) (bb2:XYPos*XYPos) =
    match fst bb1, snd bb1, fst bb2, snd bb2 with
    | (tL1, bR1, tL2, bR2) when bR2.Y < tL1.Y -> false
    | (tL1, bR1, tL2, bR2) when tL2.X > bR1.X -> false
    | (tL1, bR1, tL2, bR2) when bR2.Y > tL1.Y -> false
    | (tL1, bR1, tL2, bR2) when bR2.X < tL1.X -> false
    |_ ->true
(*
let findNearestBox (symbols: Symbol.Symbol list) (src:XYPos) =
    let list =
        Symbol.getBoundingBoxes symbols
        |> List.filter (fun (sym,l,r)-> (l.X>src.X && r.Y>src.Y && src.Y>l.Y)  ) 
    if List.isEmpty list 
        then None
    else 
        List.minBy (fun (sym,l,r)->l) list
        |> (fun (sym,l,r)->Some sym)
*)
let goPastBox (wId:CommonTypes.ConnectionId) (src:XYPos) (sym:Symbol.Symbol) =

    
    let pos1= {X=src.X; Y=src.Y}
    let pos2= {X=pos1.X;Y=sym.BotR.Y}
    [   
        makeWireSegment wId src pos1
        makeWireSegment wId pos1 pos2
        makeWireSegment wId pos2 sym.BotR
    ], sym.BotR
    
let goPastBox2 (wId:CommonTypes.ConnectionId) (src:XYPos) (sym:Symbol.Symbol) =

    
    let pos1= {X=src.X; Y=src.Y}
    let pos2= {X=pos1.X;Y=sym.BotR.Y}
    [   
        makeWireSegment wId src pos1
        makeWireSegment wId pos1 pos2
        makeWireSegment wId pos2 sym.BotR
    ], sym.BotR
    





let goToTarget (wId:CommonTypes.ConnectionId) (src:XYPos) (tar:XYPos) =
    let pos1= {X= (tar.X + src.X)/(2.) ; Y=src.Y}
    let pos2= {X=pos1.X;Y=tar.Y}
    [   
        makeWireSegment wId src pos1
        makeWireSegment wId pos1 pos2
        makeWireSegment wId pos2 tar
    ]

(*
let findBoxesInPath (symbols: Symbol.Symbol list) (src:XYPos) (tar:Symbol.Symbol)=
    let pos1= {X= (tar.TopL.X + src.X)/(2.) ; Y=src.Y}
    let pos2= {X=pos1.X;Y=tar.TopL.Y}
    let bb1= makeWireBB src pos1
    let bb2= makeWireBB pos1 pos2
    let bb3= makeWireBB pos2 tar.TopL

    let list =
        Symbol.getBoundingBoxes symbols
        |> List.map (fun (sym,l,r)->(l,r))
        |> List.filter (fun x-> (bbCollision bb1 x || bbCollision bb2 x || bbCollision bb3 x))
    if List.isEmpty list 
        then None
    else 
        Some (List.minBy (fun (l,r)->l) list)
        //|> (fun (l,r)->Some sym)
*)

let rec routing (wId:CommonTypes.ConnectionId) (symbols: Symbol.Symbol list) (src:XYPos) (tar:XYPos) =
(*
    match findNearestBox symbols src with
    | None -> goToTarget src tar
    | Some sym when sym.Id = tar.Id-> goToTarget src tar
    | Some sym -> 
        let tuple= goPastBox src sym
        ( fst tuple) @ (routing (symbols) (snd tuple) tar)
*)
    goToTarget  wId src tar

let makeWire (srcId:CommonTypes.PortId) (tarId:CommonTypes.PortId) (symbols: Symbol.Symbol list) =
    let wId= CommonTypes.ConnectionId (uuid())
    let segments = routing wId symbols (Symbol.getPortCoords symbols srcId) (Symbol.getPortCoords symbols tarId)
    {
    Id= wId 
    SrcPort= srcId
    TargetPort= tarId
    Segments= segments
    WireHighlight=false
    }

//----------------------------Message Type-----------------------------------//

/// Messages to update buswire model
/// These are OK for the demo - but not the correct messages for
/// a production system. In the real system wires must connection
/// to ports, not symbols. In addition there will be other changes needed
/// for highlighting, width inference, etc
type Msg =
    | Symbol of Symbol.Msg
    | AddWire of (CommonTypes.PortId * CommonTypes.PortId)
    | DeleteWires of CommonTypes.ConnectionId list
    | HighlightWires of CommonTypes.ConnectionId list
    | MoveWires of CommonTypes.ConnectionId list * XYPos
    | SetColor of CommonTypes.HighLightColor
    | MouseMsg of MouseT




/// look up wire in WireModel
let wire (wModel: Model) (wId: CommonTypes.ConnectionId): Wire =
    failwithf "Not impelmented"

type WireRenderProps = {
    key : CommonTypes.ConnectionId
    WireP: WireSegment
    SrcP: XYPos 
    TgtP: XYPos
    ColorP: string
    StrokeWidthP: string
    Highlight:bool
    BB: XYPos * XYPos
    }

/// react virtual DOM SVG for one wire
/// In general one wire will be multiple (right-angled) segments.

let singleWireView = 
    FunctionComponent.Of(
        fun (props: WireRenderProps) ->
            let segmentView=
                line [
                    X1 props.SrcP.X
                    Y1 props.SrcP.Y
                    X2 props.TgtP.X
                    Y2 props.TgtP.Y
                    // Qualify these props to avoid name collision with CSSProp
                    SVGAttr.Stroke (if props.Highlight then "purple" else props.ColorP)
                    SVGAttr.StrokeWidth props.StrokeWidthP ] []
            let BBview=     
                rect[
                    let h= float (snd props.BB).Y - float (fst props.BB).Y
                    let w= float (snd props.BB).X - float (fst props.BB).X
                    X (fst props.BB).X
                    Y (fst props.BB).Y
                    SVGAttr.Height (h)
                    SVGAttr.Width (w)
                    //SVGAttr.Fill "purple"
                    SVGAttr.Stroke "black"
                    SVGAttr.StrokeWidth 0.5][]
            segmentView (*:: [BBview] |> ofList*)        
            )

let view (model:Model) (dispatch: Dispatch<Msg>)=

    let wires = 
        model.WX

        |> List.map (fun w ->
            List.map (fun (segment:WireSegment)->               
                let props = {
                    key = segment.Id
                    WireP = segment
                    (*SrcP = Symbol.symbolPos model.Symbol w.SrcSymbol 
                    TgtP = Symbol. symbolPos model.Symbol w.TargetSymbol *)
                    SrcP=segment.SrcPos
                    TgtP=segment.TargetPos
                    //SrcP=w.SrcPos
                    //TgtP=w.TargetPos
                    ColorP = model.Color.Text()
                    StrokeWidthP = "2px" 
                    Highlight=w.WireHighlight
                    BB= segment.BB
                    
                    }
                
                singleWireView props) w.Segments)
    let symbols = Symbol.view model.Symbol (fun sMsg -> dispatch (Symbol sMsg))
    g [] [(g [] (List.concat wires)); symbols]

/// dummy init for testing: real init would probably start with no wires.
/// this initialisation is not realistic - ports are not used
/// this initialisation depends on details of Symbol.Model type.
    /// 


let init n () =
    let symbols, cmd = Symbol.init()
    let symIds = List.map (fun (sym:Symbol.Symbol) -> sym.Id) symbols
    let rng = System.Random 0
    (*
    let makeRandomWire() =
        let n = symIds.Length
        let s1,s2 =
            match rng.Next(0,n-1), rng.Next(0,n-2) with
            | r1,r2 when r1 = r2 -> 
                symbols.[r1],symbols.[n-1] // prevents wire target and source being same
            | r1,r2 -> 
                symbols.[r1],symbols.[r2]
        {
            Id=CommonTypes.ConnectionId (uuid())
            SrcSymbol = s1.Id
            TargetSymbol = s2.Id
        }
        *)

    // findNearestBox symbols (symbols.[0].TopL)
    // |> goPastBox (symbols.[0].TopL)

    [
        //printfn (Symbol.getPortIds symbols symbols.[0].Id)
        makeWire  ((Symbol.getPortIds symbols symbols.[0].Id).[0]) ((Symbol.getPortIds symbols symbols.[1].Id).[0]) symbols
        makeWire  ((Symbol.getPortIds symbols symbols.[2].Id).[0]) ((Symbol.getPortIds symbols symbols.[3].Id).[0]) symbols
    ]
    //List.map (fun i -> makeRandomWire()) [1..n]
    |> (fun wires -> {WX=wires;Symbol=symbols; Color=CommonTypes.Red},Cmd.none)

let isCommon lst1 lst2=
    let set1= Set.ofList lst1
    let set2 =Set.ofList lst2
    let intersect= Set.intersect set1 set2
    not (Set.isEmpty intersect)

let isSegmentInCommon (w:Wire) (segIdlst:CommonTypes.ConnectionId list)=
    let WireSegIds=List.map (fun (seg:WireSegment)->seg.Id) w.Segments
    isCommon WireSegIds segIdlst 


let update (msg : Msg) (model : Model): Model*Cmd<Msg> =
    match msg with
    | Symbol sMsg -> 
        let sm,sCmd = Symbol.update sMsg model.Symbol
        let wires = 
            model.WX
            |> List.map (fun w ->
                let segments=routing w.Id sm (Symbol.getPortCoords sm w.SrcPort) (Symbol.getPortCoords sm w.TargetPort)
                {w with Segments= segments})

        {model with Symbol=sm; WX=wires}, Cmd.map Symbol sCmd
    | AddWire (srcPort, tarPort) ->
        let w= makeWire srcPort tarPort model.Symbol
        {model with WX=w::model.WX}, Cmd.none
    | DeleteWires wIdList -> 
        let wList =
            model.WX
            |> List.filter (fun w -> List.contains w.Id wIdList = false)
        {model with WX = wList}, Cmd.none
    | HighlightWires wIdList -> 

        let wList =
            model.WX
            |> List.map (fun w ->
                if List.contains w.Id wIdList then
                    {w with WireHighlight = true}
                else 
                    {w with WireHighlight = false}
            )
        
        {model with WX = wList}, Cmd.none
    | MoveWires _ -> failwithf "Not implemented"
    | SetColor c -> {model with Color = c}, Cmd.none
    | MouseMsg mMsg -> model, Cmd.ofMsg (Symbol (Symbol.MouseMsg mMsg))

//---------------Other interface functions--------------------//
let getBoundingBoxes (wModel: Model) (mouseCoord: XYPos): (CommonTypes.ConnectionId * XYPos * XYPos) list=
    wModel.WX
    |> List.collect (fun w->
                    List.map (fun (segment:WireSegment)->
                                (segment.wId,fst segment.BB, snd segment.BB))w.Segments)


let getWireIdsFromPortIds (wModel: Model) (portIds: CommonTypes.PortId list) : CommonTypes.ConnectionId list =
    wModel.WX
    |> List.filter (fun w -> List.contains w.SrcPort portIds || List.contains w.TargetPort portIds)
    |> List.map (fun w -> w.Id)
/// Given a point on the canvas, returns the wire ID of a wire within a few pixels
/// or None if no such. Where there are two close wires the nearest is taken. Used
/// to determine which wire (if any) to select on a mouse click
let wireToSelectOpt (wModel: Model) (pos: XYPos) : CommonTypes.ConnectionId option = 
    failwith "Not implemented"

//----------------------interface to Issie-----------------------//
let extractWire (wModel: Model) (sId:CommonTypes.ComponentId) : CommonTypes.Component= 
    failwithf "Not implemented"

let extractWires (wModel: Model) : CommonTypes.Component list = 
    failwithf "Not implemented"

/// Update the symbol with matching componentId to comp, or add a new symbol based on comp.
let updateSymbolModelWithComponent (symModel: Model) (comp:CommonTypes.Component) =
    failwithf "Not Implemented"



    




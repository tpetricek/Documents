namespace VegaHub

module Basics =

    open VegaHub.Json
    open VegaHub.Grammar
    
    let scatterplot dataset (fx, fy, fc, fs) =

        let xs = Numeric("fst", fx)
        let ys = Numeric("snd", fy)
        let cs = Categorical("col", fc)
        let ss = Numeric("size", fs)

        let xScale = ("X", Width, xs)
        let yScale = ("Y", Height, ys)
        let colorScale = ("Color", Color10, cs)

        let axes = { XAxis = xScale, None; YAxis = yScale, None}

        let point = 
            {   XScale = xScale;
                XSource = Field(xs);
                YScale = yScale;
                YSource = Field(ys) }

        let decoration = { Fill = Dynamic(colorScale,cs) }
        let pointDecoration = { Size = Field(ss) }
        let mark = Symbol(point,decoration,pointDecoration)

        let template = 
            [   NVal("width",400.);
                NVal("height",400.);
                writeData dataset [xs;ys;cs;ss];
                List ("scales", [ writeScale xScale; writeScale yScale; writeScale colorScale; ]);
                writeAxes axes;
                List ("marks", [ render mark; ])
            ]

        writeObj template

    let bar dataset (fx, fy) =

        let xs = Categorical("fst", fx)
        let ys = Numeric("snd", fy)
     
        let xScale = ("X", Width, xs)
        let yScale = ("Y", Height, ys)

        let axes = { XAxis = xScale, None; YAxis = yScale, None }

        let rectangle =
            {   XSide = Relative(Field(xs), Band);
                XScale = xScale;
                YSide = Absolute(NumericValue(0.), Field(ys));
                YScale = yScale }
        let decoration = { Fill = Fixed("red") }

        let mark = Rectangle(rectangle, decoration)

        let template = 
            [   NVal("width",400.);
                NVal("height",300.);
                writeData dataset [xs;ys];
                List ("scales", [ writeScale xScale; writeScale yScale ]);
                writeAxes axes;
                List ("marks", [ render mark ])
            ]

        writeObj template

    let colorBar dataset (fx, fy, fc) =

        let xs = Categorical("fst", fx)
        let ys = Numeric("snd", fy)
        let cs = Categorical("col", fc)

        let xScale = ("X", Width, xs)
        let yScale = ("Y", Height, ys)
        let colorScale = ("Color", Color20, cs)

        let rectangle =
            {   XSide = Relative(Field(xs), Band);
                XScale = xScale;
                YSide = Absolute(NumericValue(0.), Field(ys));
                YScale = yScale }
        let decoration = { Fill = Dynamic(colorScale,cs) }
        let mark = Rectangle(rectangle, decoration)

        let axesValues = 
            let len = List.length dataset
            if len > 10
            then 
                let interval = len / 5
                let vs = dataset |> List.map fx
                Some([ for i in 0 .. (len - 1) do if (i % interval = 0) then yield vs.[i] ])
            else None
        let axes = { XAxis = xScale, axesValues; YAxis = yScale, None }

        let template = 
            [   NVal("width",400.);
                NVal("height",300.);
                writeData dataset [xs;ys;cs];
                List ("scales", [ writeScale xScale; writeScale yScale; writeScale colorScale ]);
                List ("legends", [ [Val("fill","Color")] ])
                writeAxes axes;
                List ("marks", [ render mark ])
            ]

        writeObj template

    let force nodes (ns) edges (src,tgt,v) (args:(float*float*int)) =
        
        let dist,chrg,itrs = args 

        let nodeName = Categorical("name", ns)

        let nodeExtractors = [ nodeName ]

        let edgeSource = Numeric("source", src)
        let edgeTarget = Numeric("target", tgt)
        let edgeValue = Numeric("value", v)

        let edgeExtractors = [edgeSource; edgeTarget; edgeValue]

        let connections = Path
        let bubbles = 
            [   Val("type","symbol");
                Nested("from", [ Val("data","nodes") ]);
                Nested("properties", 
                    [   Nested("enter",
                            [   Nested("x",[Val("field","x")]);
                                Nested("y",[Val("field","y")]);
                                Nested("fill",[Val("value","steelblue")]);
                                Nested("fillOpacity",[NVal("value",0.2)]);
                                Nested("stroke",[Val("value","steelblue")]);
                            ])                          
                    ])
            ]

        let template = 
            [   NVal("width",400.);
                NVal("height",300.);
                List("data", 
                    [ [ 
                        Val("name","edges"); 
                        List("values", edges |> List.map (fun x -> writeItem x edgeExtractors));
                        List("transform", [ [ Val("type","copy"); Val("from","data"); VList("fields",[ "source"; "target" ]) ] ]);
                      ]; 
                      [ 
                        Val("name","nodes"); 
                        List("values", nodes |> List.map (fun x -> writeItem x nodeExtractors));
                        List("transform", [ [ Val("type","force"); Val("links","edges"); NVal("linkDistance",dist); NVal("charge",chrg); NVal("iterations",itrs |> float) ] ]);
                      ]; 
                    ])
                List ("marks", [ render connections; bubbles ])
            ]

        writeObj template
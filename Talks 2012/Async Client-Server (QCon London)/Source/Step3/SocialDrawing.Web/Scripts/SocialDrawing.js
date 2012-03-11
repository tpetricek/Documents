registerNamespace("Pit.Async");
registerNamespace("Pit.Async.Async1");
registerNamespace("SocialDrawing.App");
registerNamespace("SocialDrawing.Dom");
registerNamespace("Pit.Async.AsyncTopLevel");
Pit.Async.Async1 = function () {
    this.Tag = 0;
    this.Item = null;
};
Pit.Async.Async1.Cont = function (item) {
    this.Item = item;
};
Pit.Async.Async1.Cont.prototype = new Pit.Async.Async1();
Pit.Async.Async1.Cont.prototype.equality = function (compareTo) {
    var result = true;
    result = result && this.get_Item() == compareTo.get_Item();
    return result;
};
Pit.Async.Async1.Cont.prototype.get_Item = function () {
    return this.Item;
};
Pit.Async.Async1.prototype.get_Tag = function () {
    return this.Tag;
};
Pit.Async.Async1.prototype.get_Item = function () {
    return this.Item;
};
Pit.Async.AsyncBuilder = (function () {
    function AsyncBuilder() {}
    AsyncBuilder.prototype.While = function (tupledArg) {
        var thisObject = this;
        var cond = tupledArg.Item1;
        var body = tupledArg.Item2;
        var x = thisObject;
        var loop = function () {
            return (function (thisObject) {
                if (cond()) {
                    return x.Bind({
                        Item1: body,
                        Item2: loop
                    });
                } else {
                    return x.Zero();
                }
            })(thisObject);
        };
        return loop();
    };
    AsyncBuilder.prototype.Bind = function (tupledArg) {
        var thisObject = this;
        var _arg1 = tupledArg.Item1;
        var f = tupledArg.Item2;
        var v = _arg1.get_Item();
        return new Pit.Async.Async1.Cont(function (k) {
            return v(function (a) {
                var patternInput = f(a);
                var r = patternInput.get_Item();
                return r(k);
            });
        });
    };
    AsyncBuilder.prototype.Delay = function (f) {
        var thisObject = this;
        return new Pit.Async.Async1.Cont(function (k) {
            var patternInput = f();
            var r = patternInput.get_Item();
            return r(k);
        });
    };
    AsyncBuilder.prototype.Zero = function () {
        var thisObject = this;
        return new Pit.Async.Async1.Cont(function (k) {
            return k();
        });
    };
    AsyncBuilder.prototype.ReturnFrom = function (w) {
        var thisObject = this;
        return w;
    };
    AsyncBuilder.prototype.Return = function (v) {
        var thisObject = this;
        return new Pit.Async.Async1.Cont(function (k) {
            return k(v);
        });
    };
    return AsyncBuilder;
})();
registerNamespace("SocialDrawing");
SocialDrawing.App.Rectangle = (function () {
    function Rectangle(x1, y1, x2, y2, color) {
        this.X1 = x1;
        this.Y1 = y1;
        this.X2 = x2;
        this.Y2 = y2;
        this.Color = color;
    }
    Rectangle.prototype.get_X1 = function () {
        return this.X1;
    };
    Rectangle.prototype.get_Y1 = function () {
        return this.Y1;
    };
    Rectangle.prototype.get_X2 = function () {
        return this.X2;
    };
    Rectangle.prototype.get_Y2 = function () {
        return this.Y2;
    };
    Rectangle.prototype.get_Color = function () {
        return this.Color;
    };
    return Rectangle;
})();
SocialDrawing.App.sendRectangle = function (value) {
    var req = new XMLHttpRequest();
    req.open("POST", "/add", true);
    return req.send(JSON.stringify(value));
};
SocialDrawing.App.getRectangles = function () {
    return (function (builder) {
        return builder.Delay(function () {
            var req = new XMLHttpRequest();
            req.open("POST", "/get", true);
            return builder.Bind({
                Item1: Pit.Async.AsyncTopLevel.XMLHttpRequest.AsyncSend(req),
                Item2: function (_arg2) {
                    var resp = _arg2;
                    return builder.Return(JSON.parse(resp));
                }
            });
        });
    })(Pit.Async.AsyncTopLevel.get_async);
};
SocialDrawing.App.createRectangle = function (rect) {
    var el = SocialDrawing.Dom.createSelection();
    el.className = "rect";
    el.style.backgroundColor = rect.get_Color();
    return SocialDrawing.Dom.moveElement(el)({
        Item1: rect.get_X1(),
        Item2: rect.get_Y1()
    })({
        Item1: rect.get_X2(),
        Item2: rect.get_Y2()
    });
};
DOM.domReady(function () {
    var div = document.getElementById("canvas");
    return Pit.FSharp.Core.Operators.op_PipeRight(Pit.FSharp.Core.Operators.op_PipeRight(div)((function (el) {
        return Pit.Dom.Event.mousedown(el);
    })))((function (sourceEvent) {
        return Pit.FSharp.Control.EventModule.Add(function (args) {
            var rect = new SocialDrawing.App.Rectangle(args.clientX, args.clientY, (args.clientX + 10), (args.clientY + 10), "red");
            SocialDrawing.App.sendRectangle(rect);
            return SocialDrawing.App.createRectangle(rect);
        })(sourceEvent);
    }));
});
SocialDrawing.Dom.createSelection = function () {
    var root = document.getElementById("canvas");
    var el = document.createElement("div");
    el.style.position = "absolute";
    root.appendChild(el);
    el.className = "selection";
    return el;
};
SocialDrawing.Dom.moveElement = function (el) {
    return function (tupledArg) {
        var x1 = tupledArg.Item1;
        var y1 = tupledArg.Item2;
        return function (tupledArg1) {
            var x2 = tupledArg1.Item1;
            var y2 = tupledArg1.Item2;
            var patternInput = {
                Item1: Pit.FSharp.Core.Operators.Min(x1)(x2),
                Item2: Pit.FSharp.Core.Operators.Min(y1)(y2)
            };
            var y = patternInput.Item2;
            var x = patternInput.Item1;
            var patternInput1 = {
                Item1: Pit.FSharp.Core.Operators.Abs((x1 - x2)),
                Item2: Pit.FSharp.Core.Operators.Abs((y1 - y2))
            };
            var w = patternInput1.Item1;
            var h = patternInput1.Item2;
            el.style.left = (x.ToString() + "px");
            el.style.top = (y.ToString() + "px");
            el.style.width = (w.ToString() + "px");
            return el.style.height = (h.ToString() + "px");
        };
    };
};
registerNamespace("Pit.Async");
Pit.Async.AsyncTopLevel.Async = (function () {
    function Async() {}
    return Async;
})();
Pit.Async.AsyncTopLevel.Async.StartImmediate = function (workflow) {
    var f = workflow.get_Item();
    return f(function () {
        return null;
    });
};
Pit.Async.AsyncTopLevel.Async.Sleep = function (milliseconds) {
    return new Pit.Async.Async1.Cont((function (k) {
        return Pit.FSharp.Core.Operators.op_PipeRight(window.setTimeout(function () {
            return k();
        },
        milliseconds))((function (value) {
            return Pit.FSharp.Core.Operators.Ignore(value);
        }));
    }));
};
Pit.Async.AsyncTopLevel.Async.AwaitEvent = function (event) {
    return new Pit.Async.Async1.Cont(function (k) {
        var hndl = null;
        hndl = function (o, _arg1) {
            var e = o;
            event.IDelegateEvent1_RemoveHandler(hndl);
            return k(e);
        };
        return event.IDelegateEvent1_AddHandler(hndl);
    });
};
Pit.Async.AsyncTopLevel.Async.AwaitEvent2 = function (tupledArg) {
    var event1 = tupledArg.Item1;
    var event2 = tupledArg.Item2;
    return new Pit.Async.Async1.Cont(function (k) {
        var event = Pit.FSharp.Control.EventModule.Merge(Pit.FSharp.Control.EventModule.Map(function (arg0) {
            return new Pit.FSharp.Core.FSharpChoice2.Choice1Of2(arg0);
        })(event1))(Pit.FSharp.Control.EventModule.Map(function (arg0) {
            return new Pit.FSharp.Core.FSharpChoice2.Choice2Of2(arg0);
        })(event2));
        var hndl = null;
        hndl = function (_arg2, e) {
            event.IDelegateEvent1_RemoveHandler(hndl);
            return k(e);
        };
        return event.IDelegateEvent1_AddHandler(hndl);
    });
};
Pit.Async.AsyncTopLevel.XMLHttpRequest = (function () {
    function XMLHttpRequest() {}
    return XMLHttpRequest;
})();
Pit.Async.AsyncTopLevel.get_async = new Pit.Async.AsyncBuilder();
Pit.Async.AsyncTopLevel.XMLHttpRequest.AsyncSend = function (x) {
    return new Pit.Async.Async1.Cont(function (k) {
        x.onreadystatechange = function () {
            return (function (thisObject) {
                if (x.readyState == 4) {
                    return k(x.responseText);
                } else {
                    return null;
                }
            })(this);
        };
        return x.send('');
    });
};
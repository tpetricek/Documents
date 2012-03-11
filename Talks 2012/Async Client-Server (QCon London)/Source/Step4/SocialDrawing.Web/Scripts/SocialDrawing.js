registerNamespace("Pit.Async");
registerNamespace("Pit.Async.Async1");
registerNamespace("SocialDrawing.App");
registerNamespace("SocialDrawing.Colors");
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
SocialDrawing.App.updateLoop = function () {
    return (function (builder) {
        return builder.Delay((function () {
            return builder.While({
                Item1: function () {
                    return true;
                },
                Item2: builder.Delay((function () {
                    return builder.Bind({
                        Item1: Pit.Async.AsyncTopLevel.Async.Sleep(500),
                        Item2: (function (_arg4) {
                            return builder.Bind({
                                Item1: SocialDrawing.App.getRectangles(),
                                Item2: function (_arg3) {
                                    var rects = _arg3;
                                    SocialDrawing.Dom.cleanRectangles();
                                    Pit.FSharp.Core.Operators.op_PipeRight(rects)((function (array) {
                                        return Pit.FSharp.Collections.ArrayModule.Iterate((function (rect) {
                                            return SocialDrawing.App.createRectangle(rect);
                                        }))(array);
                                    }));
                                    return builder.Zero();
                                }
                            });
                        })
                    });
                }))
            });
        }));
    })(Pit.Async.AsyncTopLevel.get_async);
};
SocialDrawing.App.drawing = function (selection) {
    return function (start) {
        return (function (builder) {
            return builder.Delay(function () {
                var div = document.getElementById("canvas");
                return builder.Bind({
                    Item1: Pit.Async.AsyncTopLevel.Async.AwaitEvent2({
                        Item1: Pit.Dom.Event.mouseup(div),
                        Item2: Pit.Dom.Event.mousemove(div)
                    }),
                    Item2: function (_arg5) {
                        var e = _arg5;
                        return (function (thisObject) {
                            if (e instanceof Pit.FSharp.Core.FSharpChoice2.Choice1Of2) {
                                var e1 = e.get_Item();
                                SocialDrawing.Dom.removeRectangle(selection);
                                var color = SocialDrawing.Colors.selectedColor();
                                var rect = new SocialDrawing.App.Rectangle(Pit.FSharp.Core.Operators.Fst(start), Pit.FSharp.Core.Operators.Snd(start), e1.clientX, e1.clientY, color);
                                SocialDrawing.App.sendRectangle(rect);
                                SocialDrawing.App.createRectangle(rect);
                                return builder.ReturnFrom(SocialDrawing.App.waiting());
                            } else {
                                var e1 = e.get_Item();
                                var tupledArg = {
                                    Item1: e1.clientX,
                                    Item2: e1.clientY
                                };
                                var x1 = start.Item1;
                                var y1 = start.Item2;
                                var x2 = tupledArg.Item1;
                                var y2 = tupledArg.Item2;
                                SocialDrawing.Dom.moveElement(selection)({
                                    Item1: x1,
                                    Item2: y1
                                })({
                                    Item1: x2,
                                    Item2: y2
                                });
                                return builder.ReturnFrom(SocialDrawing.App.drawing(selection)({
                                    Item1: start.Item1,
                                    Item2: start.Item2
                                }));
                            }
                        })(this);
                    }
                });
            });
        })(Pit.Async.AsyncTopLevel.get_async);
    };
};
SocialDrawing.App.waiting = function () {
    return (function (builder) {
        return builder.Delay(function () {
            var div = document.getElementById("canvas");
            return builder.Bind({
                Item1: Pit.Async.AsyncTopLevel.Async.AwaitEvent(Pit.Dom.Event.mousedown(div)),
                Item2: function (_arg6) {
                    var e = _arg6;
                    var selection = SocialDrawing.Dom.createSelection();
                    return builder.ReturnFrom(SocialDrawing.App.drawing(selection)({
                        Item1: e.clientX,
                        Item2: e.clientY
                    }));
                }
            });
        });
    })(Pit.Async.AsyncTopLevel.get_async);
};
DOM.domReady(function () {
    SocialDrawing.Colors.createPalette();
    Pit.FSharp.Core.Operators.op_PipeRight(SocialDrawing.App.updateLoop())((function (arg00) {
        return Pit.Async.AsyncTopLevel.Async.StartImmediate(arg00);
    }));
    return Pit.FSharp.Core.Operators.op_PipeRight(SocialDrawing.App.waiting())((function (arg00) {
        return Pit.Async.AsyncTopLevel.Async.StartImmediate(arg00);
    }));
});
SocialDrawing.Colors.colors = (function () {
    return Pit.FSharp.Collections.SeqModule.ToList(Pit.FSharp.Core.Operators.CreateSequence(Pit.FSharp.Collections.SeqModule.Delay(function () {
        var baseColors = new Pit.FSharp.Collections.FSharpList1.Cons({
            Item1: 136,
            Item2: 0,
            Item3: 21
        },
        new Pit.FSharp.Collections.FSharpList1.Cons({
            Item1: 237,
            Item2: 28,
            Item3: 36
        },
        new Pit.FSharp.Collections.FSharpList1.Cons({
            Item1: 255,
            Item2: 127,
            Item3: 40
        },
        new Pit.FSharp.Collections.FSharpList1.Cons({
            Item1: 255,
            Item2: 242,
            Item3: 0
        },
        new Pit.FSharp.Collections.FSharpList1.Cons({
            Item1: 34,
            Item2: 177,
            Item3: 76
        },
        new Pit.FSharp.Collections.FSharpList1.Cons({
            Item1: 0,
            Item2: 162,
            Item3: 232
        },
        new Pit.FSharp.Collections.FSharpList1.Cons({
            Item1: 63,
            Item2: 72,
            Item3: 204
        },
        new Pit.FSharp.Collections.FSharpList1.Cons({
            Item1: 163,
            Item2: 73,
            Item3: 163
        },
        new Pit.FSharp.Collections.FSharpList1.Empty()))))))));
        return Pit.FSharp.Collections.SeqModule.Append(Pit.FSharp.Collections.SeqModule.Map(function (shade) {
            return {
                Item1: shade,
                Item2: shade,
                Item3: shade
            };
        })(new Pit.FSharp.Collections.FSharpList1.Cons(32, new Pit.FSharp.Collections.FSharpList1.Cons(85, new Pit.FSharp.Collections.FSharpList1.Cons(170, new Pit.FSharp.Collections.FSharpList1.Cons(255, new Pit.FSharp.Collections.FSharpList1.Empty()))))))(Pit.FSharp.Collections.SeqModule.Delay(function () {
            return baseColors;
        }));
    })));
});
SocialDrawing.Colors.hex = function (tupledArg) {
    var r = tupledArg.Item1;
    var g = tupledArg.Item2;
    var b = tupledArg.Item3;
    var part = function (r1) {
        var letters = ["0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "a", "b", "c", "d", "e", "f"];
        return (letters[Pit.FSharp.Core.Operators.ToInt((r1 / 16))] + letters[(r1 % 16)]);
    };
    return ((("#" + part(r)) + part(g)) + part(b));
};
SocialDrawing.Colors.createPalette = function () {
    var root = document.getElementById("palette");
    var sel = document.createElement("div");
    sel.className = "selected";
    sel.id = "selected";
    sel.style.backgroundColor = "#202020";
    root.appendChild(sel);
    return Pit.FSharp.Core.Operators.op_PipeRight(Pit.FSharp.Core.Operators.op_PipeRight(SocialDrawing.Colors.colors())((function (list) {
        return Pit.FSharp.Collections.ListModule.Reverse(list);
    })))((function (list) {
        return Pit.FSharp.Collections.ListModule.Iterate(function (color) {
            var el = document.createElement("div");
            el.className = "color";
            var r = color.Item1;
            var colorName = (function (thisObject) {
                var g = color.Item2;
                var b = color.Item3;
                return SocialDrawing.Colors.hex({
                    Item1: r,
                    Item2: g,
                    Item3: b
                });
            })(this);
            el.style.backgroundColor = colorName;
            Pit.FSharp.Core.Operators.op_PipeRight(Pit.FSharp.Core.Operators.op_PipeRight(el)((function (el1) {
                return Pit.Dom.Event.click(el1);
            })))((function (sourceEvent) {
                return Pit.FSharp.Control.EventModule.Add(function (_arg1) {
                    return sel.style.backgroundColor = colorName;
                })(sourceEvent);
            }));
            return root.appendChild(el);
        })(list);
    }));
};
SocialDrawing.Colors.selectedColor = function () {
    var sel = document.getElementById("selected");
    return sel.style.backgroundColor;
};
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
SocialDrawing.Dom.cleanRectangles = function () {
    var root = document.getElementById("canvas");
    var child = root.firstChild;
    while (child != null) {
        (function (thisObject) {
            if (child.className == "rect") {
                return root.removeChild(child);
            } else {
                return null;
            }
        })(this);
        child = child.nextSibling;
    };
};
SocialDrawing.Dom.removeRectangle = function (el) {
    var root = document.getElementById("canvas");
    return root.removeChild(el);
};
registerNamespace("Pit.Async");
Pit.Async.AsyncTopLevel.Async = (function () {
    function Async() {}
    return Async;
})();
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
Pit.Async.AsyncTopLevel.Async.StartImmediate = function (workflow) {
    var f = workflow.get_Item();
    return f(function () {
        return null;
    });
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
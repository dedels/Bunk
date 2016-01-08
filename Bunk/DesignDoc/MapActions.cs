using Bunk.Design;
using Microsoft.FSharp.Collections;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bunk.DesignDoc
{
    public class MapActions<T> : IDocumentTypeFactory
    {
        private class TypeAction {
            public Type Type { get; }
            public Action<T, object> Action {get;} 
            public string ViewAction { get; }
            public TypeAction(Type type, Action<T,object> action, string viewAction) {
                this.Type = type;
                this.Action = action;
                this.ViewAction=viewAction;
            }
        }
        
        private FSharpMap<string, TypeAction> _typeMap = MapModule.Empty<string, TypeAction>();

        private MapActions()
        { }
        public static MapActions<T> Empty { get; } = new MapActions<T>();


        public MapFunction<TKey, TValue> BuildMap<TKey, TValue>()
        {
            var sb = new StringBuilder();
            sb.AppendLine("function(doc){");
            foreach(var action in this._typeMap)
            {
                sb.AppendLine($"if(doc.type==\"{action.Key}\"){{ {action.Value.ViewAction}; }}");
            }
            sb.AppendLine("}");

            return new MapFunction<TKey, TValue>(sb.ToString());
        }




        private MapActions<T> Add(Type type, string typename, string viewaction, Action<T, object> mapAction)
        {
            var newTA = new TypeAction(type, mapAction, viewaction);
            var newTypeMap = MapModule.Add(typename, newTA, this._typeMap);
            return new MapActions<T>() { _typeMap=newTypeMap };
        }



        public MapActions<T> Add<MapT>(string viewaction, Action<T,MapT> mapAction) where MapT : Document, new()
        {
            var d = new MapT();
            return this.Add(typeof(MapT), d.TYPE, viewaction, (T mapo, object newo) => { mapAction(mapo, (MapT)newo); });
        }
        


        public Type GetNetTypeFor(string typename)
        {
            return this._typeMap[typename].Type;
        }

        internal void RunMapAction(T item, GenericDocument gdoc)
        {
            var ta = this._typeMap[gdoc.TYPE];
            var doc = gdoc.ToObject(ta.Type);
            ta.Action(item, doc);
        }
    }

    


    public static class GenericFactoryViewExtensions
    {
        public static async Task<ResultT> ThroughFactory<EmitType, ResultT>(
            this Task<ViewResults<EmitType, object, GenericDocument>> resultsTask,
            MapActions<ResultT> mapActions,
            ResultT item)
        {
            return ThroughFactory(await resultsTask, mapActions, item);
        }

        public static ResultT ThroughFactory<EmitType, ResultT>(
            this ViewResults<EmitType,object,GenericDocument> results, 
            MapActions<ResultT> mapActions, 
            ResultT item)
        {
            foreach(var r in results.Rows)
            {
                mapActions.RunMapAction(item, r.Document);
            }
            return item;
        }


        public static async Task<ResultT> ThroughFactory<EmitType, ResultT>(
            this Task<ViewResults<EmitType, GenericDocument>> resultsTask,
            MapActions<ResultT> mapActions,
            ResultT item)
        {
            return ThroughFactory(await resultsTask, mapActions, item);
        }
        public static ResultT ThroughFactory<EmitType, ResultT>(
            this ViewResults<EmitType, GenericDocument> results,
            MapActions<ResultT> mapActions,
            ResultT item)
        {
            foreach (var r in results.Rows)
            {
                mapActions.RunMapAction(item, r.Value);
            }
            return item;
        }
    }
}

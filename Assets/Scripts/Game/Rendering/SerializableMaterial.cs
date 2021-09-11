using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using UnityEngine;

namespace RTS.Game.Rendering
{
    public class SerializableMaterial
    {
        // 材质的着色器
        public string shader;
        // 是否是在现有的材质上更新一部分属性（还是完全重新设置一个新的材质）
        public bool updateExisting;
        // 材质参数
        public Dictionary<string, Value> values;

        // 创建一个能用来修改一个现有的 MaterialPropertyBlock 的函数
        // 这个 SerializableMaterial 的所有材质参数都通过这个创建的函数，来进行设置
        public Action<MaterialPropertyBlock> CreateSetter(MaterialsManager materialsManager)
        {
            var list = new List<Expression<Action<MaterialPropertyBlock>>>();

            foreach (var kv in values)
            {
                var key = kv.Key;
                var value = kv.Value;
                switch (value.value)
                {
                    case string id when value.type is Value.Type.Texture:
                        list.Add(x => x.SetTexture(key, materialsManager.RetrieveTexture(id)));
                        break;
                    case int @int:
                        list.Add(x => x.SetInt(key, @int));
                        break;
                    case float @float:
                        list.Add(x => x.SetFloat(key, @float));
                        break;
                    case Color color:
                        list.Add(x => x.SetColor(key, color));
                        break;
                    case Vector4 vector:
                        list.Add(x => x.SetVector(key, vector));
                        break;
                    case Matrix4x4 matrix:
                        list.Add(x => x.SetMatrix(key, matrix));
                        break;
                    default:
                        throw new NotSupportedException();
                }
            }

            Expression<Action<MaterialPropertyBlock>> result = x => x.Clear();
            var parameter = ((MethodCallExpression)result.Body).Object;
            var actions = from expression in list
                          let call = (MethodCallExpression)expression.Body
                          select call.Update(parameter, call.Arguments) as Expression;
            if (!updateExisting)
            {
                // 在函数的一开始，添加一个“Clear”的调用
                actions = actions.Prepend(result.Body);
            }
            return result.Update(Expression.Block(actions), result.Parameters).Compile();
        }
    }

    public struct Value
    {
        public enum Type : int
        {
            None,
            Texture,
            Color,
            Vector,
            Matrix,
            Float,
            Int
        }

        public Type type;
        public object value;

        public System.Type GetValueType() => type switch
        {
            Type.None => null,
            Type.Texture => typeof(string),
            Type.Color => typeof(Color),
            Type.Vector => typeof(Vector4),
            Type.Matrix => typeof(Matrix4x4),
            Type.Float => typeof(float),
            Type.Int => typeof(int),
            _ => throw new NotSupportedException()
        };
    }

    public class ValueConverter : JsonConverter<Value>
    {
        public override Value ReadJson(JsonReader reader, Type objectType, Value existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var jo = JObject.Load(reader);
            var value = new Value
            {
                type = jo[nameof(Value.type)].ToObject<Value.Type>()
            };
            var type = value.GetValueType();
            if (type != null)
            {
                value.value = jo[nameof(Value.value)].ToObject(type);
            }
            return value;
        }

        public override void WriteJson(JsonWriter writer, Value value, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            writer.WritePropertyName(nameof(Value.type));
            serializer.Serialize(writer, value.type);
            writer.WritePropertyName(nameof(Value.value));
            serializer.Serialize(writer, value.value);
            writer.WriteEndObject();
        }
    }
}
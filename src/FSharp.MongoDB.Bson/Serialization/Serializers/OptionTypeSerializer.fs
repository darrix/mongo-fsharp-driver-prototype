(* Copyright (c) 2013 MongoDB, Inc.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *)

namespace FSharp.MongoDB.Bson.Serialization.Serializers

open Microsoft.FSharp.Reflection

open MongoDB.Bson.Serialization
open MongoDB.Bson.Serialization.Serializers

/// <summary>
/// A serializer for option types.
/// Writes the value for the <c>Some</c> case, and <c>null</c> for the None case.
/// </summary>
type OptionTypeSerializer<'typ>() =
    inherit SerializerBase<'typ option>()

    let typ = typeof<'typ option>
    let cases = FSharpType.GetUnionCases(typ) |> Seq.map (fun x -> (x.Name, x)) |> dict

    override __.Serialize(context, args, value) =
        let writer = context.Writer
        let value = Some (typ.GetProperty("Value").GetValue(value, [| |]))

        match value with
        | Some x -> BsonSerializer.Serialize(writer, x.GetType(), x)
        | None -> BsonSerializer.Serialize(writer, typeof<obj>, null)

    override __.Deserialize(context, args) =
        let reader = context.Reader
        let value = BsonSerializer.Deserialize(reader, typ.GenericTypeArguments.[0])

        let (case, args) =
            match value with
            | null -> (cases.["None"], [| |])
            | _ -> (cases.["Some"], [| value |])

        FSharpValue.MakeUnion(case, args) :?> 'typ option

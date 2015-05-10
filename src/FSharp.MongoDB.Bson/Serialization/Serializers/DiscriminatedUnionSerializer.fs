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

open MongoDB.Bson.IO
open MongoDB.Bson.Serialization
open MongoDB.Bson.Serialization.Serializers

/// <summary>
/// A serializer for discriminated unions.
/// Handles null union cases.
/// </summary>
type DiscriminatedUnionSerializer<'typ>() =
    inherit SerializerBase<'typ>()

    let isUnion typ = FSharpType.IsUnion typ

    let typ = typeof<'typ>
    let cases = FSharpType.GetUnionCases(typ) |> Seq.map (fun x -> (x.Name, x)) |> dict

    let nameDecoder = Utf8NameDecoder()

    override __.Serialize(context, args, value) =
        let writer = context.Writer
        let (case, fields) = FSharpValue.GetUnionFields(value, typ)

        // determine whether name is a null union case or not
        match case.GetFields() with
        | [| |] ->
            writer.WriteStartDocument()

            writer.WriteName "_t" // TODO: base element name off convention
            writer.WriteString case.Name

            writer.WriteEndDocument()

        | _ ->
            // defer to the class map
            let classMap = BsonClassMap.LookupClassMap typeof<'typ>
            BsonClassMapSerializer(classMap :?> BsonClassMap<'typ>).Serialize(context, args, value)

    override __.Deserialize(context, args) =
        let reader = context.Reader
        let mark = reader.GetBookmark()

        reader.ReadStartDocument()

        let discriminator = reader.ReadName nameDecoder
        if discriminator <> "_t" then // TODO: base element name off convention
            failwithf "Expected _t discriminator, but got %A" discriminator

        let name = reader.ReadString()
        let union = cases.[name]

        // determine whether name is a null union case or not
        match union.GetFields() with
        | [| |] ->
            reader.ReadEndDocument()
            FSharpValue.MakeUnion(union, [| |]) :?> 'typ

        | _ ->
            let case = typ.GetNestedTypes() |> Array.filter isUnion |> Array.find (fun x -> x.Name = name)

            reader.ReturnToBookmark mark
            BsonSerializer.Deserialize(reader, case) :?> 'typ // defer to the class map

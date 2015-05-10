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

namespace FSharp.MongoDB.Bson.Serialization

open Microsoft.FSharp.Reflection

open MongoDB.Bson.Serialization

open FSharp.MongoDB.Bson.Serialization.Serializers

/// Provides (de)serialization of F# data types.
/// Includes options, lists, maps, sets, records, and discriminated unions.
type FSharpValueSerializationProvider() =

    let isUnion typ = FSharpType.IsUnion typ

    let isOption typ = isUnion typ && typ.IsGenericType
                                   && typ.GetGenericTypeDefinition() = typedefof<_ option>

    let isList typ = isUnion typ && typ.IsGenericType
                                 && typ.GetGenericTypeDefinition() = typedefof<_ list>

    let isMap (typ : System.Type) =
        typ.IsGenericType && typ.GetGenericTypeDefinition() = typedefof<Map<_, _>>

    let isSet (typ : System.Type) =
        typ.IsGenericType && typ.GetGenericTypeDefinition() = typedefof<Set<_>>

    interface IBsonSerializationProvider with
        member __.GetSerializer(typ : System.Type) =

            // Check that `typ` is an option type
            if isOption typ then
                typedefof<OptionTypeSerializer<_>>.MakeGenericType (typ.GetGenericArguments())
                |> System.Activator.CreateInstance
                :?> IBsonSerializer

            // Check that `typ` is a list type
            elif isList typ then
                typedefof<FSharpListSerializer<_>>.MakeGenericType (typ.GetGenericArguments())
                |> System.Activator.CreateInstance
                :?> IBsonSerializer

            // Check that `typ` is a map type
            elif isMap typ then
                typedefof<FSharpMapSerializer<_, _>>.MakeGenericType (typ.GetGenericArguments())
                |> System.Activator.CreateInstance
                :?> IBsonSerializer

            // Check that `typ` is a set type
            elif isSet typ then
                typedefof<FSharpSetSerializer<_>>.MakeGenericType (typ.GetGenericArguments())
                |> System.Activator.CreateInstance
                :?> IBsonSerializer

            // Check that `typ` is the overall union type, and not a particular union case
            elif isUnion typ && typ.BaseType = typeof<obj> then
                let nested = typ.GetNestedTypes() |> Array.filter isUnion
                let props = typ.GetProperties() |> Array.filter (fun x -> isUnion x.PropertyType)

                // Handles non-singleton discriminated unions
                if nested.Length > 0 || props.Length > 0 then
                    nested |> Array.iter (fun x -> BsonClassMap.LookupClassMap x |> ignore)
                    typedefof<DiscriminatedUnionSerializer<_>>.MakeGenericType [| typ |]
                    |> System.Activator.CreateInstance
                    :?> IBsonSerializer

                // Handles singleton discriminated unions
                else null

            // Otherwise, signal we do not provide serialization for this type
            else null

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
[<RequireQualifiedAccess>]
/// <summary>
/// Entry point to initialize the <see cref="FSharpValueSerializationProvider" />.
/// </summary>
module Serializers =

    let mutable private registered = false

    [<CompiledName("Register")>]
    /// <summary>Registers the <see cref="FSharpValueSerializationProvider" />.</summary>
    let register() =
        if not registered then
            registered <- true
            BsonSerializer.RegisterSerializationProvider(FSharpValueSerializationProvider())

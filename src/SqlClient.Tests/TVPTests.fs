﻿module FSharp.Data.TVPTests

open FSharp.Data
open Xunit

// If compile fails here, check prereqs.sql
type TableValuedTuple = SqlCommandProvider<"exec Person.myProc @x", ConnectionStrings.AdventureWorksNamed, SingleRow = true>
type MyTableType = TableValuedTuple.MyTableType

[<Fact>]
let Basic() = 
    let cmd = new TableValuedTuple()
    let p = [
        MyTableType(myId = 1, myName = Some "monkey")
        MyTableType(myId = 2, myName = Some "donkey")
    ]
    let actual = cmd.Execute(x = p).Value
    Assert.Equal(1, actual.myId)
    Assert.Equal(Some "monkey", actual.myName)

[<Fact>] 
let InputIsEnumeratedExactlyOnce() = 
    let cmd = new TableValuedTuple()
    let counter = ref 0
    let x = seq { 
         counter := !counter + 1
         yield MyTableType(myId = 1)
         yield MyTableType(myId = 2, myName = Some "donkey")
    }
    cmd.Execute x |> ignore
    Assert.Equal(1, !counter)    

[<Fact>] 
let NullableColumn() = 
    let cmd = new TableValuedTuple()
    let p = [
        MyTableType(myId = 1)
        MyTableType(myId = 2, myName = Some "donkey")
    ]
    let actual = cmd.Execute(p).Value
    Assert.Equal(1, actual.myId)
    Assert.Equal(None, actual.myName)


type TableValuedSingle = SqlCommandProvider<"exec SingleElementProc @x", ConnectionStringOrName = ConnectionStrings.AdventureWorksNamed>

[<Fact>]
let SingleColumn() = 
    let cmd = new TableValuedSingle()
    let p = [ 
        TableValuedSingle.SingleElementType(myId = 1) 
        TableValuedSingle.SingleElementType(myId = 2) 
    ]
    let result = cmd.Execute(x = p) |> List.ofSeq
    Assert.Equal<int list>([1;2], result)    

[<Fact>]
let tvpSqlParamCleanUp() = 
    let cmd = new TableValuedSingle()
    let p = [ 
        TableValuedSingle.SingleElementType(myId = 1) 
        TableValuedSingle.SingleElementType(myId = 2) 
    ]
    cmd.Execute(x = p) |> List.ofSeq |> ignore
    let result = cmd.Execute(x = p) |> List.ofSeq
    Assert.Equal<int list>([1;2], result)    

type TableValuedSprocTuple  = SqlCommandProvider<"exec Person.myProc @x", ConnectionStringOrName = ConnectionStrings.AdventureWorksNamed, SingleRow = true>

[<Fact>]
let SprocTupleValue() = 
    let cmd = new TableValuedSprocTuple()
    let p = [
        TableValuedSprocTuple.MyTableType(myId = 1, myName = Some "monkey")
        TableValuedSprocTuple.MyTableType(myId = 2, myName = Some "donkey")
    ]
    let actual = cmd.Execute(p).Value
    Assert.Equal(1, actual.myId)
    Assert.Equal(Some "monkey", actual.myName)

type TableValuedTupleWithOptionalParams = SqlCommandProvider<"exec Person.myProc @x", ConnectionStrings.AdventureWorksNamed, AllParametersOptional = true>
[<Fact>]
let TableValuedTupleWithOptionalParams() = 
    let cmd = new TableValuedTupleWithOptionalParams()
    cmd.Execute Array.empty |> ignore
(*
    don't delete this test. The previous line fails with if combo of TVP and AllParametersOptional = true is not handled properly
Error	1	The type provider 'FSharp.Data.SqlCommandProvider' reported an error in the context of provided type 'FSharp.Data.SqlCommandProvider,CommandText="exec myProc @x",ConnectionStringOrName="name=AdventureWorks2012",AllParametersOptional="True"', member 'Execute'. 
The error: Value cannot be null.	C:\Users\mitekm\Documents\GitHub\FSharp.Data.SqlClient\src\SqlClient.Tests\TVPTests.fs	83	5	SqlClient.Tests
*)
   
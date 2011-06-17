namespace CardWall
open System
open System.Collections.Generic
open System.Data
open System.Linq

type ObjectDataReader<'T> =
  val items : IEnumerator<'T>
  val fields : Dictionary<String, int>
  val keys : List<'T -> obj>
  val types : List<Type>

  new(collection) = {
    items = (collection : IEnumerable<'T>).GetEnumerator()
    fields = Dictionary()
    keys = List()
    types = List<Type>() }

  member this.Item with get name = this.[this.fields.[name]]
  member this.Item with get (index:int) = this.keys.[index] this.items.Current
  member this.FieldCount = this.fields.Count
  member this.GetName i = this.fields.Single(fun x -> x.Value = i).Key

  member this.Read() = this.items.MoveNext()

  member this.AddMember(name, f:'T -> DateTime) =
    this.fields.Add(name, this.keys.Count)
    this.keys.Add(fun x -> 
        let value = f x
        if value = DateTime.MinValue then
            null
        else box value)
    this.types.Add(typeof<DateTime>)

  member this.AddMember(name, f:'T -> 'TResult) =
    this.fields.Add(name, this.keys.Count)
    this.keys.Add(f >> box)
    this.types.Add(typeof<'TResult>)

  member private this.NotSupported() = raise(NotSupportedException())

  interface IDataReader with
    member this.Depth = 0
    member this.IsClosed = false
    member this.RecordsAffected = 0
    member this.Close() = ()
    member this.NextResult() = false
    member this.Dispose() = this.items.Dispose()
    member this.FieldCount = this.FieldCount
    member this.Item with get (index:int) = this.[index]
    member this.Item with get (name:string) = this.[name]
    member this.GetName i = this.GetName i
    member this.GetDataTypeName i = this.types.[i].Name
    member this.GetFieldType i = this.types.[i]
    member this.GetValue i = this.[i]
    member this.GetValues values = 
      let len = Math.Max(values.Length, this.keys.Count)
      for i = 0 to len - 1 do
        values.[i] <- this.[i]
      len
    member this.GetOrdinal name = this.fields.[name]
    member this.GetBoolean i = this.NotSupported()
    member this.GetByte i = this.NotSupported()
    member this.GetChar i = this.NotSupported()
    member this.GetGuid i = this.NotSupported()
    member this.GetInt16 i = this.NotSupported()
    member this.GetInt32 i = this.NotSupported()
    member this.GetInt64 i = this.NotSupported()
    member this.GetFloat i = this.NotSupported()
    member this.GetDouble i = this.NotSupported()
    member this.GetString i = this.NotSupported()
    member this.GetDecimal i = this.NotSupported()
    member this.GetDateTime i = this.NotSupported()
    member this.GetData i = this.NotSupported()
    member this.IsDBNull i = this.NotSupported()
    member this.Read() = this.Read()
    member this.GetSchemaTable() = this.NotSupported()
    member this.GetBytes(i, fieldOffset, buffer, bufferOffset, length) = this.NotSupported()
    member this.GetChars(i, fieldOffset, buffer, bufferOffset, length) = this.NotSupported()
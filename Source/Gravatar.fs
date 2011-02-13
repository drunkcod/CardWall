namespace CardWall

open System
open System.Security.Cryptography
open System.Text

module Gravatar =
    [<CompiledName("FromEmail")>]
    let fromEmail (emailAddress:string) =
        let md5 = new MD5CryptoServiceProvider()
        let result = 
            md5.ComputeHash(Encoding.ASCII.GetBytes(emailAddress.Trim().ToLowerInvariant()))
            |> Seq.fold (fun (result:StringBuilder) x -> result.AppendFormat("{0:x2}", x)) (StringBuilder()) 

        String.Format("http://gravatar.com/avatar/{0}?s=48&d=monsterid", result)

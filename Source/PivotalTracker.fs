namespace CardWall

open System
open System.Net
open System.Xml.Serialization
open System.Xml.XPath
open System.Threading.Tasks
open System.Collections.Generic

type PivotalProjectMember = {
    Name : string
    EmailAddress : string
    Initials : string
}

type PivotalProject = {
    Id : int
    Name : string
}

type PivotalTracker(trackerToken) =
    let BaseUrl = "http://www.pivotaltracker.com/services/v3"

    member private this.CreateRequest path = 
        let r = WebRequest.Create(BaseUrl + path) :?> HttpWebRequest
        r.Headers.Add("X-TrackerToken", trackerToken)
        r

    member this.XmlRequest path = 
        let request = this.CreateRequest path
        let toXml x = 
            let response = request.EndGetResponse(x)
            use stream = response.GetResponseStream()
            XPathDocument(stream).CreateNavigator()

        Task.Factory.FromAsync((fun a b -> request.BeginGetResponse(a, b)), toXml, null)

    member private this.GetStories path =
        let request = this.XmlRequest(path)
        request.ContinueWith(fun (task : Task<XPathNavigator>) -> 
            task.Result
            |> XPath.map "//story" (fun x -> x.ReadSubtree() |> Xml.read (PivotalStory())))

    member this.Projects() =
        let request = this.XmlRequest("/projects")
        request.ContinueWith(fun (task : Task<XPathNavigator>) ->
            task.Result
            |> XPath.map "//project" (fun x ->
                let nodeValue xpath = x.NodeValueOrDefault(xpath, "")
                { Id = int(nodeValue "id"); Name = nodeValue "name" }))

    member this.ProjectMembers(project:int) =
        let request = this.XmlRequest(String.Format("/projects/{0}/memberships", project))
        request.ContinueWith(fun (task : Task<XPathNavigator>) ->
            task.Result
            |> XPath.map "//person" (fun x ->
                let nodeValue xpath = x.NodeValueOrDefault(xpath, "")
                { Name = nodeValue "name"; EmailAddress = nodeValue "email"; Initials = nodeValue "initials" }))

    member this.CurrentIteration(project:int) = this.GetStories(String.Format("/projects/{0}/iterations/current", project))

    member this.Stories(project:int) = this.GetStories(String.Format("/projects/{0}/stories", project))

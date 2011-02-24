namespace CardWall

open System
open System.Net
open System.Xml.XPath
open System.Threading.Tasks
open System.Collections.Generic

type PivotalProjectMember = {
    Name : string
    EmailAddress : string
    Initials : string
}

type PivotalStory = {
    ProjectId : int
    Type : string
    Estimate : Nullable<int>
    CurrentState : string
    Url : string
    Name : string
    OwnedBy : string
    Labels : string[]
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

    member this.Projects() =
        let request = this.XmlRequest("/projects")
        request.ContinueWith(fun (task : Task<XPathNavigator>) ->
            task.Result
            |> XPath.map "//project" (fun x ->
                let nodeValue xpath = x.NodeValueOrDefault(xpath, "")
                { Id = int(nodeValue "id"); Name = nodeValue "name" }))

    member this.ProjectMembers (project:int) =
        let request = this.XmlRequest(String.Format("/projects/{0}/memberships", project))
        request.ContinueWith(fun (task : Task<XPathNavigator>) ->
            task.Result
            |> XPath.map "//person" (fun x ->
                let nodeValue xpath = x.NodeValueOrDefault(xpath, "")
                { Name = nodeValue "name"; EmailAddress = nodeValue "email"; Initials = nodeValue "initials" }))

    member this.CurrentIteration (project:int) =
        let request = this.XmlRequest(String.Format("/projects/{0}/iterations/current", project))
        let intOrNull x = 
            if String.IsNullOrEmpty(x) then Nullable()
            else Nullable(int(x))
        request.ContinueWith(fun (task : Task<XPathNavigator>) -> 
            task.Result
            |> XPath.map "//story" (fun x ->
                let nodeValue xpath = x.NodeValueOrDefault(xpath, "")
                { ProjectId = int(nodeValue "project_id"); Type = nodeValue "story_type"; Estimate = intOrNull(nodeValue "estimate"); CurrentState = nodeValue "current_state"; Url = nodeValue "url"; Name = nodeValue "name"; OwnedBy = nodeValue "owned_by"; Labels = (nodeValue "labels").Split([|','|]) }))

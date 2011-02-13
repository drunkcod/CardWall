<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<CardWall.Models.CardView>" %>
<div class="<%=Model.Type%> card">
    <h1><%=Model.Title %></h1>
    <div class='card-summary'><%=Model.Summary %></div>
    <div class='card-footer'><%=Model.Owner %></div>
    <%if(!string.IsNullOrEmpty(Model.AvatarUrl)) { %>
    <img class='card-avatar' src="<%=Model.AvatarUrl %>" />
    <%} %>
    <img class='card-type' src='/Content/PivotalTracker/<%=Model.Type%>.png'/>
</div>

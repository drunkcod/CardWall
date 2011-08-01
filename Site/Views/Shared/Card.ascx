<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<CardWall.Models.CardView>" %>
<div class="<%=Model.Type%> card<%if(Model.Size.HasValue){%> size<%=Model.Size%><%} %> <%if(Model.HasFooter){%> with-footer<%}%>" id="<%=Model.Id%>" data-type="<%=Model.Type%>" data-size="<%=Model.Size%>">
    <h1 class="card-title"><a href="<%=Model.Url%>" target="_blank"><%=Model.Title %></a></h1>
    <div class="card-project-name"><%=Model.ProjectName %></div>
    <%if(Model.ShowSummary) { %>
    <div class='card-summary'><%=Model.Summary %></div>
    <% } %>
    <%if(Model.HasFooter) { %>
    <div class='card-footer'><%=Model.Owner %><br />
        <span class='card-labels'><%foreach(var item in Model.Labels){ %><%=item%>&nbsp; <%} %></span>
    </div>
    <%} %>
    <%if(Model.Tasks.Count != 0){ %>
    <div class="card-tasks">
        <%=Model.TasksDone%>/<%=Model.Tasks.Count%> tasks done
        <div class="hidden task-list"><ul><%foreach(var item in Model.Tasks) { %>
        <li><img title="<%=item.Name%>" src="<%=Url.Content(item.ImageUrl)%>" /><%=item.Name %></li><%} %></ul>
        </div>
    </div>
    <%} %>

    <%if(!string.IsNullOrEmpty(Model.AvatarUrl)) { %>
    <img class='card-avatar' src="<%=Url.Content(Model.AvatarUrl) %>" />
    <%} %>
    <ul class="card-badges"><%foreach(var item in Model.Badges) { %>
        <li><img title="<%=item.Name%>" src="<%=Url.Content(item.ImageUrl)%>" /></li>
    <%}%></ul>
</div>
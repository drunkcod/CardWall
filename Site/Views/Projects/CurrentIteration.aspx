<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<CardWall.Models.IterationView>" %>
<%@ Import Namespace="CardWall.Models" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>Iteration Overview</title>
    <link rel="stylesheet" type="text/css" href="<%=Url.Content("~/Content/Style.css")%>" />
	<script src="<%=Url.Content("~/Content/Scripts/jquery-1.5.2.min.js")%>" type="text/javascript"></script>
	<script src="<%=Url.Content("~/Content/Scripts/jquery.tmpl.min.js")%>" type="text/javascript"></script>
</head>
<body>
  <table id="content" cellpadding=0 cellspacing=0 border=0>
    <thead>
        <%foreach(var lane in Model.Lanes){ %>
            <th class="lane-header"><%=lane.Name %></th>
        <%} %>
    </thead>
    <tr>
        <%foreach(var lane in Model.Lanes){ %>
        <td class="lane" valign="top">
            <%foreach(var card in lane.Cards) {
                Html.RenderPartial("Card", card);
            } %>
        </td>
        <%} %>
    </tr>
  </table>

  <div id="actions">
	<ul>
		<li><button class="print">Print selected</button></li>
	</ul>
  </div>
  <div id="print-view"></div>
  <script id="print-view-template" type="text/x-jquery-tmpl">
	{{each(idx, card) cards}}
	<div class="card ${breakClass(idx)}">
		<img src="<%=Url.Content("~/Content/Print")%>/${type}.png"/>
		<h1>${title}</h1>
		<img class="qr" src="https://chart.googleapis.com/chart?chs=100x100&cht=qr&choe=UTF-8&chld=L|0&chl=${url}"/>
		<div class="size">${size}</div>
		<div class="id">${id}</div>
	</div>
	{{/each}}
  </script>

  <script type="text/javascript">
	var breakClass = function (index){
		switch (index % 4) {			case 0:			case 2:				return 'break';			case 3:	return 'page-break';			default: return '';		}	};

	$('#content .card').click(function(event){
		$(this).toggleClass('selected');
		$('#content .card.selected').length ? $('#actions').slideDown() : $('#actions').slideUp();
	});

	$('#actions .print').click(function(event){
		var cards = $.map($('#content .card.selected'), function(card){
			var elm = $(card);
			return {
				id:elm.attr('id'),
				title:$('h1 a', elm).text(),
				size:elm.attr('data-size'),
				type:elm.attr('data-type'),
				url:$('h1 a', elm).attr('href')
			};
		});
		$('#actions').hide();
		$('#content').fadeOut();
		$('#print-view-template').tmpl({cards:cards}).appendTo($('#print-view').empty().fadeIn());
		$('body').css('background-color', '#fff');
	});
  </script>
</body>
</html>

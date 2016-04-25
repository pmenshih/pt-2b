<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="404.aspx.cs" Inherits="pt_2b._404" %>
<% Response.StatusCode = 404; %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Страница не найдена</title>
</head>
<body>
    <h3>
    Страница не найдена
    </h3>
    <div>
        Вернуться <a href="/">на главную</a>.
    </div>
</body>
</html>

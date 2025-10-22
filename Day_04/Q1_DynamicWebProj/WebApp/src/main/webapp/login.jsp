<%@ page language="java" contentType="text/html; charset=UTF-8" pageEncoding="UTF-8"%>
<!DOCTYPE html>
<html>
<head>
  <title>Login Page</title>
</head>
<body>
  <h2>Login Form</h2>
  <form action="LoginServlet" method="post">
    <label>Login ID:</label>
    <input type="text" name="login_id" required /><br/><br/>

    <label>Password:</label>
    <input type="password" name="password" required /><br/><br/>

    <input type="submit" value="Login" />
  </form>
</body>
</html>
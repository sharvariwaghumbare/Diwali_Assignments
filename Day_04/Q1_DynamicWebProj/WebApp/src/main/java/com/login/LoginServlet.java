package com.login;

import java.io.*;
import javax.servlet.*;
import javax.servlet.http.*;
import java.sql.*;

public class LoginServlet extends HttpServlet {
  protected void doPost(HttpServletRequest request, HttpServletResponse response)
      throws ServletException, IOException {

    System.out.println("🚀 LoginServlet triggered");

    String loginId = request.getParameter("login_id");
    String password = request.getParameter("password");

    loginId = loginId != null ? loginId.trim() : "";
    password = password != null ? password.trim() : "";

    System.out.println("Login ID received: " + loginId);
    System.out.println("Password received: " + password);

    boolean isValid = false;

    try {
      Class.forName("com.mysql.cj.jdbc.Driver");
      Connection con = DriverManager.getConnection(
        "jdbc:mysql://localhost:3306/userdb", "root", "sharvari");

      PreparedStatement ps = con.prepareStatement(
        "SELECT * FROM users WHERE login_id=? AND password=?");
      ps.setString(1, loginId);
      ps.setString(2, password);

      ResultSet rs = ps.executeQuery();

      if (rs.next()) {
        System.out.println("✅ Match found in DB");
        isValid = true;
      } else {
        System.out.println("❌ No match found in DB");
      }

      rs.close();
      ps.close();
      con.close();
    } catch (Exception e) {
      System.out.println("🚨 Exception occurred:");
      e.printStackTrace();
    }

    if (isValid) {
      RequestDispatcher rd = request.getRequestDispatcher("welcome.jsp");
      rd.forward(request, response);
    } else {
      PrintWriter out = response.getWriter();
      out.println("<font color='red'>Invalid login. Try again.</font>");
      RequestDispatcher rd = request.getRequestDispatcher("login.jsp");
      rd.include(request, response);
    }
  }
}
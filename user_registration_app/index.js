const express = require("express");
const bodyParser = require("body-parser");
const cors = require("cors");
const sql = require("mssql");

const app = express();
app.use(cors());
app.use(bodyParser.json());

// SQL Server config for named instance and Windows Auth
const dbConfig = {
    server: "LAPTOP-RAUSR2S1",   // your PC name
    database: "MyFirstDB",        // your DB
    options: {
        trustServerCertificate: true
    },
    instanceName: "User"          // your named instance
    // For Windows Authentication, no user/password is needed
};

app.post("/register", async (req, res) => {
    const { first_name, last_name, email, password } = req.body;

    if (!first_name || !last_name || !email || !password) {
        return res.status(400).json({ error: "All fields are required" });
    }

    try {
        let pool = await sql.connect(dbConfig);

        await pool.request()
            .input("first_name", sql.NVarChar(50), first_name)
            .input("last_name", sql.NVarChar(50), last_name)
            .input("email", sql.NVarChar(100), email)
            .input("password", sql.NVarChar(100), password)
            .query(`INSERT INTO Users (first_name, last_name, email, password)
                    VALUES (@first_name, @last_name, @email, @password)`);

        res.send("✅ User registered successfully!");
    } catch (err) {
        console.error("❌ DB Connection Error:", err);
        res.status(500).send("Error registering user");
    }
});

app.listen(3000, () => {
    console.log("🚀 Server running on http://localhost:3000");
});

const express = require("express");
const bodyParser = require("body-parser");
const cors = require("cors");
const sql = require("mssql/msnodesqlv8"); // ✅ Use this driver

const app = express();
app.use(cors());
app.use(bodyParser.json());

const dbConfig = {
    server: "localhost\\User",   // your instance name (LAPTOP-RAUSR2S1\User)
    database: "MyFirstDB",
    driver: "msnodesqlv8",
    options: {
        trustedConnection: true
    }
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

        res.send({ message: "✅ User registered successfully!" });
    } catch (err) {
        console.error("❌ DB Connection Error:", err);
        res.status(500).send({ error: "Error registering user" });
    }
});

app.listen(3000, () => {
    console.log("🚀 Server running on http://localhost:3000");
});

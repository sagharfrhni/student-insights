import { useState } from "react";
import LoginPage from "./pages/LoginPage";
import RegisterPage from "./pages/RegisterPage";

export default function App() {
  const [currentPage, setCurrentPage] = useState("login");

  return (
    <div>
      <div style={{ display: "flex", gap: "10px", justifyContent: "center", marginTop: "20px" }}>
        <button onClick={() => setCurrentPage("login")}>Login</button>
        <button onClick={() => setCurrentPage("register")}>Register</button>
      </div>

      {currentPage === "login" ? <LoginPage /> : <RegisterPage />}
    </div>
  );
}

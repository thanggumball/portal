import { useAuth } from "@/hooks/auth.hook";
import path from "@/constants/path";
import { useNavigate } from "react-router";
import { useState } from "react";

export default function Login() {
    const { login, loading, error } = useAuth();
    const navigate = useNavigate();

    const [email, setEmail] = useState("");
    const [password, setPassword] = useState("");

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();

        const data = await login(email, password);

        if (!data) return;

        // Login success → Main
        navigate(path.home);
    };

    return (
        <form onSubmit={handleSubmit}>
            <input
                value={email}
                onChange={(e) => setEmail(e.target.value)}
            />

            <input
                type="password"
                value={password}
                onChange={(e) => setPassword(e.target.value)}
            />

            <button disabled={loading}>
                {loading ? "Đang đăng nhập..." : "Đăng nhập"}
            </button>

        </form>
    );
}
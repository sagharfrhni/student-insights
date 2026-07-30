import React, { useEffect, useMemo, useRef, useState } from "react";
import "./AdminPage.css";

const USE_REAL_API = false;

// ... (توابع apiFetchUsers و apiToggleUserStatus ثابت می‌مانند) ...

function getMockUsers() {
  return [
    { id: 1, fullName: "دانشجوی تستی", email: "student@test.com", role: "دانشجو", isActive: true, createdAt: "2026-03-01T12:00:00Z" },
    { id: 2, fullName: "مدیر سیستم", email: "admin@test.com", role: "مدیر", isActive: true, createdAt: "2026-03-02T08:30:00Z" },
    { id: 3, fullName: "کاربر غیرفعال", email: "inactive@test.com", role: "دانشجو", isActive: false, createdAt: "2026-03-10T10:00:00Z" },
  ];
}

// ... (توابع کمکی فرمت‌دهی ثابت می‌مانند) ...

export default function AdminPage() {
  const [users, setUsers] = useState([]);
  const [loading, setLoading] = useState(true);
  const [busyUserIds, setBusyUserIds] = useState(() => new Set());
  const [error, setError] = useState("");
  const [toast, setToast] = useState(null);

  const tableBodyRef = useRef(null);
  const activeCount = useMemo(() => users.filter((u) => u.isActive).length, [users]);

  const showToast = (type, message) => {
    setToast({ type, message });
    window.clearTimeout(showToast._t);
    showToast._t = window.setTimeout(() => setToast(null), 3000);
  };

  const loadUsers = async () => {
    setLoading(true);
    setError("");
    try {
      const data = USE_REAL_API ? await apiFetchUsers() : getMockUsers();
      setUsers(Array.isArray(data) ? data : data?.users || []);
    } catch (e) {
      setError(e?.message || "خطا در دریافت اطلاعات کاربران.");
      setUsers([]);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => { loadUsers(); }, []);

  // Event Delegation
  useEffect(() => {
    const el = tableBodyRef.current;
    if (!el) return;

    const onClick = async (event) => {
      const btn = event.target?.closest?.('button[data-action="toggle-user-status"]');
      if (!btn) return;

      const userId = Number(btn.getAttribute("data-user-id"));
      const user = users.find((u) => Number(u.id) === userId);
      if (!user || busyUserIds.has(userId)) return;

      const nextIsActive = !user.isActive;
      const confirmMsg = `آیا مطمئن هستید که می‌خواهید وضعیت کاربر ${user.fullName} را به "${nextIsActive ? 'فعال' : 'غیرفعال'}" تغییر دهید؟`;
      
      if (!window.confirm(confirmMsg)) return;

      setBusyUserIds((prev) => new Set(prev).add(userId));
      setUsers((prev) => prev.map((u) => (Number(u.id) === userId ? { ...u, isActive: nextIsActive } : u)));

      try {
        if (USE_REAL_API) await apiToggleUserStatus(userId, nextIsActive);
        showToast("success", `وضعیت با موفقیت تغییر کرد.`);
      } catch (e) {
        setUsers((prev) => prev.map((u) => (Number(u.id) === userId ? { ...u, isActive: !nextIsActive } : u)));
        showToast("error", "خطا در برقراری ارتباط با سرور.");
      } finally {
        setBusyUserIds((prev) => { const copy = new Set(prev); copy.delete(userId); return copy; });
      }
    };

    el.addEventListener("click", onClick);
    return () => el.removeEventListener("click", onClick);
  }, [users, busyUserIds]);

  return (
    <div className="admin-page" style={{ padding: 16, direction: 'rtl', textAlign: 'right' }}>
      <header className="admin-header" style={{ marginBottom: 16 }}>
        <h1 style={{ margin: 0 }}>پنل مدیریت</h1>
        <p style={{ margin: "6px 0 0", opacity: 0.8 }}>مدیریت کاربران و دسترسی‌های سیستم (MVP)</p>
      </header>

      {/* Toast */}
      {toast && <div className={`toast ${toast.type}`} style={{ padding: 10, background: '#e0f7fa', marginBottom: 10 }}>{toast.message}</div>}

      <section className="admin-toolbar" style={{ display: "flex", gap: 12, alignItems: "center", marginBottom: 12 }}>
        <button onClick={loadUsers} disabled={loading} className="btn">
          {loading ? "در حال بارگذاری..." : "تازه‌سازی"}
        </button>

        <div style={{ marginLeft: "auto", display: "flex", gap: 15, opacity: 0.9 }}>
          <span>کل کاربران: <b>{users.length}</b></span>
          <span>فعال: <b>{activeCount}</b></span>
          <span>غیرفعال: <b>{Math.max(users.length - activeCount, 0)}</b></span>
        </div>
      </section>

      {error && <div style={{ padding: 12, background: "#fff2f2", color: '#d32f2f', borderRadius: 10 }}>خطا: {error}</div>}

      <section className="admin-users-section" style={{ marginTop: 12 }}>
        <div style={{ overflowX: "auto", border: "1px solid #eaeaea", borderRadius: 12 }}>
          <table style={{ width: "100%", borderCollapse: "collapse", minWidth: 820 }}>
            <thead>
              <tr style={{ textAlign: "right", background: "#fafafa" }}>
                <th style={{ padding: "10px 12px", borderBottom: "1px solid #eee" }}>شناسه</th>
                <th style={{ padding: "10px 12px", borderBottom: "1px solid #eee" }}>نام کامل</th>
                <th style={{ padding: "10px 12px", borderBottom: "1px solid #eee" }}>ایمیل</th>
                <th style={{ padding: "10px 12px", borderBottom: "1px solid #eee" }}>نقش</th>
                <th style={{ padding: "10px 12px", borderBottom: "1px solid #eee" }}>وضعیت</th>
                <th style={{ padding: "10px 12px", borderBottom: "1px solid #eee" }}>تاریخ عضویت</th>
                <th style={{ padding: "10px 12px", borderBottom: "1px solid #eee" }}>عملیات</th>
              </tr>
            </thead>

            {/* ✅ قرارداد: tbody با این id */}
            <tbody id="admin-users-table-body" ref={tableBodyRef}>
              {loading ? (
                <tr><td colSpan={7} style={{ padding: 14 }}>در حال دریافت اطلاعات...</td></tr>
              ) : users.length === 0 ? (
                <tr><td colSpan={7} style={{ padding: 14 }}>کاربری یافت نشد.</td></tr>
              ) : (
                users.map((u) => (
                  <tr key={u.id} data-user-id={u.id} style={{ borderTop: "1px solid #f1f1f1" }}>
                    <td style={{ padding: "10px 12px" }}>{u.id}</td>
                    <td style={{ padding: "10px 12px" }}>{u.fullName}</td>
                    <td style={{ padding: "10px 12px" }}>{u.email}</td>
                    <td style={{ padding: "10px 12px" }}>{u.role}</td>
                    <td style={{ padding: "10px 12px" }}>{u.isActive ? "فعال" : "غیرفعال"}</td>
                    <td style={{ padding: "10px 12px" }}>{new Date(u.createdAt).toLocaleDateString('fa-IR')}</td>
                    <td style={{ padding: "10px 12px" }}>
                      <button
                        data-action="toggle-user-status"
                        data-user-id={u.id}
                        className="btn-toggle-status"
                        style={{ padding: "5px 15px", cursor: 'pointer' }}
                      >
                        {u.isActive ? "غیرفعال‌سازی" : "فعال‌سازی"}
                      </button>
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>
      </section>
    </div>
  );
}

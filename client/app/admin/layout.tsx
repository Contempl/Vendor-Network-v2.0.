"use client";

import { useEffect, useState } from "react";
import { useRouter } from "next/navigation";

export default function AdminLayout({ children }: { children: React.ReactNode }) {
  const router = useRouter();
  const [checked, setChecked] = useState(false);

  useEffect(() => {
    if (!localStorage.getItem("tkn-tko")) {
      router.push("/admin/login");
    } else {
      setChecked(true);
    }
  }, []);

  if (!checked) return null;
  return <>{children}</>;
}

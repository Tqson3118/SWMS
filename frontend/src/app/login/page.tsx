import { redirect } from "next/navigation";

export default async function LoginRedirectPage({
  searchParams,
}: {
  searchParams: Promise<Record<string, string | string[] | undefined>>;
}) {
  const resolvedParams = await searchParams;
  const query = new URLSearchParams();

  for (const [key, value] of Object.entries(resolvedParams)) {
    if (Array.isArray(value)) {
      for (const item of value) {
        if (typeof item === "string") query.append(key, item);
      }
    } else if (typeof value === "string") {
      query.set(key, value);
    }
  }

  const qs = query.toString();
  redirect(qs ? `/auth?${qs}` : "/auth");
}

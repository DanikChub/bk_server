type Props = { children: React.ReactNode; tone?: "green" | "blue" | "gray" | "orange" };

export function StatusBadge({ children, tone = "green" }: Props) {
  return <span className={`status-badge status-badge--${tone}`}>{children}</span>;
}

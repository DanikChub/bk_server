/* eslint-disable @next/next/no-img-element */
import { CircleHelp, Home, Mail, Phone } from "lucide-react";

const fallbackIcons = { home: Home, tickets: Mail, about: CircleHelp, hotline: Phone };
type DesignIconName = keyof typeof fallbackIcons | "customers" | "contracts" | "administration" | "library" | "collapse" | "bell" | "logout";

export function DesignIcon({ name }: { name: DesignIconName }) {
  if (name in fallbackIcons) {
    const Icon = fallbackIcons[name as keyof typeof fallbackIcons];
    return <Icon className="design-icon" size={14} aria-hidden="true" />;
  }
  return <img className="design-icon" src={`/design/shell/${name}.png`} alt="" />;
}

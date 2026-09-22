import type { ThemeConfig } from 'antd';

const hsl = (name: string): string => {
  const raw = getComputedStyle(document.documentElement)
    .getPropertyValue(`--${name}`)
    .trim();
  return raw ? `hsl(${raw})` : '';
};

export function buildAntdTheme(): ThemeConfig['token'] {
  return {
    colorPrimary:      hsl('primary'),
    colorInfo:         hsl('primary'),
    colorSuccess:      hsl('primary'),        // no `success` in palette; freemapping
    colorWarning:      hsl('accent'),         // pick a token or add one to SCSS
    colorError:        hsl('destructive'),
    colorTextBase:     hsl('foreground'),
    colorBgBase:       hsl('background'),
    colorBgContainer:  hsl('card'),
    colorBgElevated:   hsl('popover'),
    colorBorder:       hsl('border'),
    colorBorderSecondary: hsl('border'),
  };
}
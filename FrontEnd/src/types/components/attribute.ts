import type { Rule } from 'antd/es/form';

export type FieldType =
  | 'string' | 'number' | 'boolean' | 'checkbox'
  | 'select' | 'date' | 'year' | 'month' | 'textarea';

export interface SelectOption {
  value: string | number;
  label: string;
}

interface BaseAttribute {
  key: string;
  title: string;
  readonly?: boolean;
  rules?: Rule[];
  width?: number | string;
  align?: 'left' | 'center' | 'right';
}

export interface StringAttribute   extends BaseAttribute { type: 'string';   addOnAfter?: string; placeholder?: string; }
export interface NumberAttribute   extends BaseAttribute { type: 'number';   addOnAfter?: string; placeholder?: string; }
export interface BooleanAttribute  extends BaseAttribute { type: 'boolean'; }
export interface CheckboxAttribute extends BaseAttribute { type: 'checkbox'; }
export interface SelectAttribute   extends BaseAttribute { type: 'select';   options: SelectOption[]; defaultValue?: string | number; }
export interface DateAttribute     extends BaseAttribute { type: 'date'; }
export interface YearAttribute     extends BaseAttribute { type: 'year'; }
export interface MonthAttribute    extends BaseAttribute { type: 'month'; }
export interface TextAreaAttribute extends BaseAttribute { type: 'textarea'; rowSize?: number; }

export type Attribute =
  | StringAttribute | NumberAttribute | BooleanAttribute | CheckboxAttribute
  | SelectAttribute | DateAttribute | YearAttribute | MonthAttribute | TextAreaAttribute;

export interface Filter {
  key: string;
  value: unknown;
}
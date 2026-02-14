// src/shared/ui/Spinner/Spinner.tsx

import React from 'react';
import styles from './Spinner.module.css';
import { cn } from '../../utils';

export type SpinnerSize = 'xs' | 'sm' | 'md' | 'lg' | 'xl';
export type SpinnerVariant = 'default' | 'primary' | 'light';

export interface SpinnerProps {
  size?: SpinnerSize;
  variant?: SpinnerVariant;
  label?: string;
  className?: string;
}

export const Spinner: React.FC<SpinnerProps> = ({
  size = 'md',
  variant = 'default',
  label,
  className,
}) => {
  return (
    <div className={cn(styles.container, className)}>
      <div className={cn(styles.spinner, styles[size], styles[variant])}>
        <div className={styles.ring1} />
        <div className={styles.ring2} />
        <div className={styles.ring3} />
      </div>
      {label && <span className={styles.label}>{label}</span>}
    </div>
  );
};

// Simple spinner variant
export const SimpleSpinner: React.FC<{ size?: SpinnerSize; className?: string }> = ({
  size = 'md',
  className,
}) => (
  <div className={cn(styles.simple, styles[size], className)} />
);

export default Spinner;
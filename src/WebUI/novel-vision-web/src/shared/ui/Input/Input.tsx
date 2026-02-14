// src/shared/ui/Input/Input.tsx

import React, { forwardRef, useState } from 'react';
import { motion } from 'framer-motion';
import styles from './Input.module.css';
import { cn } from '../../utils';

export interface InputProps extends Omit<React.InputHTMLAttributes<HTMLInputElement>, 'size'> {
  label?: string;
  error?: string;
  hint?: string;
  icon?: React.ReactNode;
  iconPosition?: 'left' | 'right';
  size?: 'sm' | 'md' | 'lg';
  fullWidth?: boolean;
  clearable?: boolean;
  onClear?: () => void;
}

export const Input = forwardRef<HTMLInputElement, InputProps>(
  (
    {
      label,
      error,
      hint,
      icon,
      iconPosition = 'left',
      size = 'md',
      fullWidth = false,
      clearable = false,
      onClear,
      className,
      type = 'text',
      disabled,
      value,
      ...props
    },
    ref
  ) => {
    const [showPassword, setShowPassword] = useState(false);
    const isPassword = type === 'password';
    const hasValue = value !== undefined && value !== '';

    return (
      <div className={cn(styles.wrapper, fullWidth ? styles.fullWidth : undefined, className)}>
        {label && (
          <label className={styles.label}>
            {label}
            {props.required && <span className={styles.required}>*</span>}
          </label>
        )}

        <div
          className={cn(
            styles.inputWrapper,
            styles[size],
            error ? styles.hasError : undefined,
            disabled ? styles.disabled : undefined,
            icon && iconPosition === 'left' ? styles.hasIconLeft : undefined,
            icon && iconPosition === 'right' ? styles.hasIconRight : undefined
          )}
        >
          {icon && iconPosition === 'left' && (
            <span className={cn(styles.icon, styles.iconLeft)}>{icon}</span>
          )}

          <input
            ref={ref}
            type={isPassword && showPassword ? 'text' : type}
            className={styles.input}
            disabled={disabled}
            value={value}
            {...props}
          />

          {icon && iconPosition === 'right' && !isPassword && !clearable && (
            <span className={cn(styles.icon, styles.iconRight)}>{icon}</span>
          )}

          {isPassword && (
            <button
              type="button"
              className={styles.togglePassword}
              onClick={() => setShowPassword(!showPassword)}
              tabIndex={-1}
            >
              {showPassword ? '👁️' : '👁️‍🗨️'}
            </button>
          )}

          {clearable && hasValue && !isPassword && (
            <button
              type="button"
              className={styles.clearButton}
              onClick={onClear}
              tabIndex={-1}
            >
              ✕
            </button>
          )}
        </div>

        {(error || hint) && (
          <motion.p
            className={cn(styles.message, error ? styles.errorMessage : undefined)}
            initial={{ opacity: 0, y: -5 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ duration: 0.15 }}
          >
            {error || hint}
          </motion.p>
        )}
      </div>
    );
  }
);

Input.displayName = 'Input';

export default Input;
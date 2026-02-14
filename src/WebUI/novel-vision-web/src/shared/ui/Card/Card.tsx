// src/shared/ui/Card/Card.tsx

import React from 'react';
import { motion, HTMLMotionProps } from 'framer-motion';
import styles from './Card.module.css';
import { cn } from '../../utils';

export type CardVariant = 'default' | 'glass' | 'elevated' | 'bordered' | 'gradient';
export type CardPadding = 'none' | 'sm' | 'md' | 'lg';

export interface CardProps extends Omit<HTMLMotionProps<'div'>, 'children'> {
  children: React.ReactNode;
  variant?: CardVariant;
  padding?: CardPadding;
  hover?: boolean;
  clickable?: boolean;
  glow?: boolean;
}

export const Card: React.FC<CardProps> = ({
  children,
  variant = 'default',
  padding = 'md',
  hover = false,
  clickable = false,
  glow = false,
  className,
  ...props
}) => {
  return (
    <motion.div
      className={cn(
        styles.card,
        styles[variant],
        styles[`padding-${padding}`],
        hover ? styles.hover : undefined,
        clickable ? styles.clickable : undefined,
        glow ? styles.glow : undefined,
        className
      )}
      whileHover={hover ? { y: -4 } : undefined}
      whileTap={clickable ? { scale: 0.98 } : undefined}
      transition={{ duration: 0.2 }}
      {...props}
    >
      {children}
    </motion.div>
  );
};

// Sub-components
export const CardHeader: React.FC<{ children: React.ReactNode; className?: string }> = ({
  children,
  className,
}) => <div className={cn(styles.header, className)}>{children}</div>;

export const CardBody: React.FC<{ children: React.ReactNode; className?: string }> = ({
  children,
  className,
}) => <div className={cn(styles.body, className)}>{children}</div>;

export const CardFooter: React.FC<{ children: React.ReactNode; className?: string }> = ({
  children,
  className,
}) => <div className={cn(styles.footer, className)}>{children}</div>;

export default Card;
// src/shared/ui/Dropdown/Dropdown.tsx

import React, { useState, useRef, useEffect } from 'react';
import { motion, AnimatePresence } from 'framer-motion';
import styles from './Dropdown.module.css';
import { cn } from '../../utils';

export interface DropdownItem {
  id: string;
  label: string;
  icon?: React.ReactNode;
  onClick?: () => void;
  disabled?: boolean;
  danger?: boolean;
  divider?: boolean;
}

export interface DropdownProps {
  trigger: React.ReactNode;
  items: DropdownItem[];
  align?: 'left' | 'right';
  width?: number | string;
  className?: string;
}

export const Dropdown: React.FC<DropdownProps> = ({
  trigger,
  items,
  align = 'left',
  width = 200,
  className,
}) => {
  const [isOpen, setIsOpen] = useState(false);
  const ref = useRef<HTMLDivElement>(null);

  useEffect(() => {
    const handleClickOutside = (e: MouseEvent) => {
      if (ref.current && !ref.current.contains(e.target as Node)) {
        setIsOpen(false);
      }
    };
    document.addEventListener('mousedown', handleClickOutside);
    return () => document.removeEventListener('mousedown', handleClickOutside);
  }, []);

  const handleItemClick = (item: DropdownItem) => {
    if (item.disabled || item.divider) return;
    item.onClick?.();
    setIsOpen(false);
  };

  return (
    <div ref={ref} className={cn(styles.dropdown, className)}>
      <div onClick={() => setIsOpen(!isOpen)}>{trigger}</div>
      
      <AnimatePresence>
        {isOpen && (
          <motion.div
            className={cn(styles.menu, styles[align])}
            style={{ width }}
            initial={{ opacity: 0, y: -10, scale: 0.95 }}
            animate={{ opacity: 1, y: 0, scale: 1 }}
            exit={{ opacity: 0, y: -10, scale: 0.95 }}
            transition={{ duration: 0.15 }}
          >
            {items.map((item) =>
              item.divider ? (
                <div key={item.id} className={styles.divider} />
              ) : (
                <button
                  key={item.id}
                  className={cn(
                    styles.item,
                    item.disabled ? styles.disabled : undefined,
                    item.danger ? styles.danger : undefined
                  )}
                  onClick={() => handleItemClick(item)}
                  disabled={item.disabled}
                >
                  {item.icon && <span className={styles.icon}>{item.icon}</span>}
                  <span>{item.label}</span>
                </button>
              )
            )}
          </motion.div>
        )}
      </AnimatePresence>
    </div>
  );
};

export default Dropdown;
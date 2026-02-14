// src/shared/ui/Avatar/Avatar.tsx

import React from 'react';
import styles from './Avatar.module.css';
import { cn } from '../../utils';

export type AvatarSize = 'xs' | 'sm' | 'md' | 'lg' | 'xl';
export type AvatarStatus = 'online' | 'offline' | 'away' | 'busy';

export interface AvatarProps {
  src?: string;
  name?: string;
  size?: AvatarSize;
  status?: AvatarStatus;
  className?: string;
}

const getInitials = (name: string): string => {
  const parts = name.trim().split(' ');
  if (parts.length >= 2) {
    return `${parts[0][0]}${parts[1][0]}`.toUpperCase();
  }
  return name.substring(0, 2).toUpperCase();
};

const getColorFromName = (name: string): string => {
  const colors = [
    'linear-gradient(135deg, #a78bfa, #6366f1)',
    'linear-gradient(135deg, #f472b6, #ec4899)',
    'linear-gradient(135deg, #34d399, #10b981)',
    'linear-gradient(135deg, #60a5fa, #3b82f6)',
    'linear-gradient(135deg, #fbbf24, #f59e0b)',
    'linear-gradient(135deg, #f87171, #ef4444)',
  ];
  let hash = 0;
  for (let i = 0; i < name.length; i++) {
    hash = name.charCodeAt(i) + ((hash << 5) - hash);
  }
  return colors[Math.abs(hash) % colors.length];
};

export const Avatar: React.FC<AvatarProps> = ({
  src,
  name = '',
  size = 'md',
  status,
  className,
}) => {
  const [imgError, setImgError] = React.useState(false);
  const showImage = src && !imgError;

  return (
    <div className={cn(styles.avatar, styles[size], className)}>
      {showImage ? (
        <img
          src={src}
          alt={name}
          className={styles.image}
          onError={() => setImgError(true)}
        />
      ) : (
        <div
          className={styles.initials}
          style={{ background: getColorFromName(name) }}
        >
          {getInitials(name || '?')}
        </div>
      )}
      {status && <span className={cn(styles.status, styles[status])} />}
    </div>
  );
};

// Avatar Group
export interface AvatarGroupProps {
  children: React.ReactNode;
  max?: number;
  size?: AvatarSize;
}

export const AvatarGroup: React.FC<AvatarGroupProps> = ({
  children,
  max = 4,
  size = 'md',
}) => {
  const childArray = React.Children.toArray(children);
  const visible = childArray.slice(0, max);
  const remaining = childArray.length - max;

  return (
    <div className={styles.group}>
      {visible.map((child, i) => (
        <div key={i} className={styles.groupItem} style={{ zIndex: max - i }}>
          {React.isValidElement(child)
            ? React.cloneElement(child as React.ReactElement<AvatarProps>, { size })
            : child}
        </div>
      ))}
      {remaining > 0 && (
        <div className={cn(styles.avatar, styles[size], styles.remaining)}>
          +{remaining}
        </div>
      )}
    </div>
  );
};

export default Avatar;
// src/pages/ResetPasswordPage/ResetPasswordPage.tsx
// Password reset completion page (with token from email link)

import React, { useState, useEffect } from 'react';
import { Link, useNavigate, useSearchParams } from 'react-router-dom';
import { motion } from 'framer-motion';
import { Button } from '../../shared/ui/Button';
import { Input } from '../../shared/ui/Input';
import { Card } from '../../shared/ui/Card';
import { Spinner } from '../../shared/ui/Spinner';
import { ROUTES } from '../../shared/constants/routes';
import { authService } from '../../features/auth/services/authService';
import styles from './ResetPasswordPage.module.css';

type PageState = 'loading' | 'form' | 'success' | 'error' | 'expired';

interface PasswordStrength {
  score: number;
  label: string;
  color: string;
}

const ResetPasswordPage: React.FC = () => {
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();

  const token = searchParams.get('token');
  const email = searchParams.get('email');

  const [pageState, setPageState] = useState<PageState>('loading');
  const [password, setPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [showPassword, setShowPassword] = useState(false);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');

  // Validate token on mount
  useEffect(() => {
    if (!token || !email) {
      setPageState('error');
      return;
    }
    // Token validation happens on submit - just show form
    setPageState('form');
  }, [token, email]);

  // Calculate password strength
  const getPasswordStrength = (pwd: string): PasswordStrength => {
    let score = 0;
    if (pwd.length >= 8) score++;
    if (pwd.length >= 12) score++;
    if (/[A-Z]/.test(pwd)) score++;
    if (/[a-z]/.test(pwd)) score++;
    if (/[0-9]/.test(pwd)) score++;
    if (/[^A-Za-z0-9]/.test(pwd)) score++;

    if (score <= 2) return { score, label: 'Weak', color: 'var(--error)' };
    if (score <= 4) return { score, label: 'Medium', color: 'var(--warning)' };
    return { score, label: 'Strong', color: 'var(--success)' };
  };

  const passwordStrength = getPasswordStrength(password);

  // Validation
  const validateForm = (): boolean => {
    if (password.length < 8) {
      setError('Password must be at least 8 characters');
      return false;
    }
    if (!/[A-Z]/.test(password)) {
      setError('Password must contain at least one uppercase letter');
      return false;
    }
    if (!/[a-z]/.test(password)) {
      setError('Password must contain at least one lowercase letter');
      return false;
    }
    if (!/[0-9]/.test(password)) {
      setError('Password must contain at least one number');
      return false;
    }
    if (password !== confirmPassword) {
      setError('Passwords do not match');
      return false;
    }
    return true;
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');

    if (!validateForm()) return;

    setLoading(true);

    try {
      await authService.resetPassword(token!, email!, password);
      setPageState('success');
    } catch (err: any) {
      console.error('Password reset failed:', err);
      if (err.response?.status === 400) {
        setError('This reset link has expired. Please request a new one.');
        setPageState('expired');
      } else {
        setError('Failed to reset password. Please try again.');
      }
    } finally {
      setLoading(false);
    }
  };

  // Loading State
  if (pageState === 'loading') {
    return (
      <div className={styles.page}>
        <div className={styles.container}>
          <Spinner size="lg" label="Validating reset link..." />
        </div>
      </div>
    );
  }

  // Error State (invalid link)
  if (pageState === 'error') {
    return (
      <div className={styles.page}>
        <div className={styles.container}>
          <Card variant="glass" padding="lg" className={styles.card}>
            <div className={styles.stateContent}>
              <span className={styles.stateIcon}>❌</span>
              <h2 className={styles.stateTitle}>Invalid Reset Link</h2>
              <p className={styles.stateText}>
                This password reset link is invalid or malformed.
                Please request a new one.
              </p>
              <Link to={ROUTES.FORGOT_PASSWORD}>
                <Button variant="primary">Request New Link</Button>
              </Link>
            </div>
          </Card>
        </div>
      </div>
    );
  }

  // Expired State
  if (pageState === 'expired') {
    return (
      <div className={styles.page}>
        <div className={styles.container}>
          <Card variant="glass" padding="lg" className={styles.card}>
            <div className={styles.stateContent}>
              <span className={styles.stateIcon}>⏰</span>
              <h2 className={styles.stateTitle}>Link Expired</h2>
              <p className={styles.stateText}>
                This password reset link has expired for security reasons.
                Please request a new one.
              </p>
              <Link to={ROUTES.FORGOT_PASSWORD}>
                <Button variant="primary">Request New Link</Button>
              </Link>
            </div>
          </Card>
        </div>
      </div>
    );
  }

  // Success State
  if (pageState === 'success') {
    return (
      <div className={styles.page}>
        <div className={styles.container}>
          <Card variant="glass" padding="lg" className={styles.card}>
            <div className={styles.stateContent}>
              <span className={styles.stateIcon}>✅</span>
              <h2 className={styles.stateTitle}>Password Reset!</h2>
              <p className={styles.stateText}>
                Your password has been successfully reset.
                You can now sign in with your new password.
              </p>
              <Button variant="primary" onClick={() => navigate(ROUTES.LOGIN)}>
                Sign In Now
              </Button>
            </div>
          </Card>
        </div>
      </div>
    );
  }

  // Form State
  return (
    <div className={styles.page}>
      <div className={styles.container}>
        {/* Logo */}
        <motion.div
          className={styles.logo}
          initial={{ opacity: 0, y: -20 }}
          animate={{ opacity: 1, y: 0 }}
        >
          <Link to={ROUTES.HOME} className={styles.logoLink}>
            <span className={styles.logoIcon}>🔮</span>
            <span className={styles.logoText}>NovelVision</span>
          </Link>
        </motion.div>

        <motion.div
          initial={{ opacity: 0, y: 20 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ delay: 0.1 }}
        >
          <Card variant="glass" padding="lg" className={styles.card}>
            <div className={styles.header}>
              <span className={styles.headerIcon}>🔑</span>
              <h1 className={styles.title}>Set New Password</h1>
              <p className={styles.subtitle}>
                Create a strong password for <strong>{email}</strong>
              </p>
            </div>

            <form onSubmit={handleSubmit} className={styles.form}>
              {error && (
                <motion.div
                  className={styles.errorMessage}
                  initial={{ opacity: 0, height: 0 }}
                  animate={{ opacity: 1, height: 'auto' }}
                >
                  ⚠️ {error}
                </motion.div>
              )}

              {/* New Password */}
              <div className={styles.inputGroup}>
                <label className={styles.label}>New Password</label>
                <div className={styles.passwordWrapper}>
                  <Input
                    type={showPassword ? 'text' : 'password'}
                    placeholder="Enter new password"
                    value={password}
                    onChange={(e) => setPassword(e.target.value)}
                    disabled={loading}
                    autoFocus
                  />
                  <button
                    type="button"
                    className={styles.togglePassword}
                    onClick={() => setShowPassword(!showPassword)}
                  >
                    {showPassword ? '🙈' : '👁️'}
                  </button>
                </div>

                {/* Password Strength */}
                {password && (
                  <div className={styles.strengthBar}>
                    <div
                      className={styles.strengthFill}
                      style={{
                        width: `${(passwordStrength.score / 6) * 100}%`,
                        backgroundColor: passwordStrength.color,
                      }}
                    />
                    <span
                      className={styles.strengthLabel}
                      style={{ color: passwordStrength.color }}
                    >
                      {passwordStrength.label}
                    </span>
                  </div>
                )}
              </div>

              {/* Confirm Password */}
              <div className={styles.inputGroup}>
                <label className={styles.label}>Confirm Password</label>
                <Input
                  type={showPassword ? 'text' : 'password'}
                  placeholder="Confirm new password"
                  value={confirmPassword}
                  onChange={(e) => setConfirmPassword(e.target.value)}
                  disabled={loading}
                />
                {confirmPassword && password !== confirmPassword && (
                  <span className={styles.mismatch}>Passwords don't match</span>
                )}
                {confirmPassword && password === confirmPassword && (
                  <span className={styles.match}>✓ Passwords match</span>
                )}
              </div>

              {/* Requirements */}
              <div className={styles.requirements}>
                <p className={styles.reqTitle}>Password requirements:</p>
                <ul className={styles.reqList}>
                  <li className={password.length >= 8 ? styles.met : ''}>
                    At least 8 characters
                  </li>
                  <li className={/[A-Z]/.test(password) ? styles.met : ''}>
                    One uppercase letter
                  </li>
                  <li className={/[a-z]/.test(password) ? styles.met : ''}>
                    One lowercase letter
                  </li>
                  <li className={/[0-9]/.test(password) ? styles.met : ''}>
                    One number
                  </li>
                </ul>
              </div>

              <Button
                type="submit"
                variant="primary"
                fullWidth
                disabled={loading || password !== confirmPassword || password.length < 8}
                className={styles.submitButton}
              >
                {loading ? 'Resetting...' : '🔒 Reset Password'}
              </Button>
            </form>
          </Card>
        </motion.div>

        <motion.p
          className={styles.helpText}
          initial={{ opacity: 0 }}
          animate={{ opacity: 1 }}
          transition={{ delay: 0.3 }}
        >
          Remember your password? <Link to={ROUTES.LOGIN} className={styles.helpLink}>Sign In</Link>
        </motion.p>
      </div>
    </div>
  );
};

export default ResetPasswordPage;
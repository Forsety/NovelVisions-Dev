// src/pages/ForgotPasswordPage/ForgotPasswordPage.tsx
// Password recovery request page

import React, { useState } from 'react';
import { Link } from 'react-router-dom';
import { motion } from 'framer-motion';
import { Button } from '../../shared/ui/Button';
import { Input } from '../../shared/ui/Input';
import { Card } from '../../shared/ui/Card';
import { ROUTES } from '../../shared/constants/routes';
import { authService } from '../../features/auth/services/authService';
import styles from './ForgotPasswordPage.module.css';

type PageState = 'form' | 'success' | 'error';

const ForgotPasswordPage: React.FC = () => {
  const [email, setEmail] = useState('');
  const [pageState, setPageState] = useState<PageState>('form');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');

  const validateEmail = (email: string): boolean => {
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return emailRegex.test(email);
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');

    if (!email.trim()) {
      setError('Please enter your email address');
      return;
    }

    if (!validateEmail(email)) {
      setError('Please enter a valid email address');
      return;
    }

    setLoading(true);

    try {
      await authService.forgotPassword(email);
      setPageState('success');
    } catch (err: any) {
      console.error('Password reset request failed:', err);
      // Don't reveal if email exists or not for security
      setPageState('success');
    } finally {
      setLoading(false);
    }
  };

  const handleRetry = () => {
    setPageState('form');
    setEmail('');
    setError('');
  };

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

        {/* Form State */}
        {pageState === 'form' && (
          <motion.div
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ delay: 0.1 }}
          >
            <Card variant="glass" padding="lg" className={styles.card}>
              <div className={styles.header}>
                <span className={styles.headerIcon}>🔐</span>
                <h1 className={styles.title}>Forgot Password?</h1>
                <p className={styles.subtitle}>
                  No worries! Enter your email and we'll send you reset instructions.
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

                <div className={styles.inputGroup}>
                  <label className={styles.label}>Email Address</label>
                  <Input
                    type="email"
                    placeholder="you@example.com"
                    value={email}
                    onChange={(e) => setEmail(e.target.value)}
                    disabled={loading}
                    autoFocus
                  />
                </div>

                <Button
                  type="submit"
                  variant="primary"
                  fullWidth
                  disabled={loading}
                  className={styles.submitButton}
                >
                  {loading ? 'Sending...' : '📧 Send Reset Link'}
                </Button>
              </form>

              <div className={styles.footer}>
                <Link to={ROUTES.LOGIN} className={styles.backLink}>
                  ← Back to Sign In
                </Link>
              </div>
            </Card>
          </motion.div>
        )}

        {/* Success State */}
        {pageState === 'success' && (
          <motion.div
            initial={{ opacity: 0, scale: 0.95 }}
            animate={{ opacity: 1, scale: 1 }}
          >
            <Card variant="glass" padding="lg" className={styles.card}>
              <div className={styles.successContent}>
                <span className={styles.successIcon}>✉️</span>
                <h2 className={styles.successTitle}>Check Your Email</h2>
                <p className={styles.successText}>
                  If an account exists for <strong>{email}</strong>, you'll receive
                  a password reset link shortly.
                </p>
                <p className={styles.successHint}>
                  Don't see it? Check your spam folder or try again.
                </p>

                <div className={styles.successActions}>
                  <Button variant="outline" onClick={handleRetry}>
                    Try Different Email
                  </Button>
                  <Link to={ROUTES.LOGIN}>
                    <Button variant="primary">Back to Sign In</Button>
                  </Link>
                </div>
              </div>
            </Card>
          </motion.div>
        )}

        {/* Help Text */}
        <motion.p
          className={styles.helpText}
          initial={{ opacity: 0 }}
          animate={{ opacity: 1 }}
          transition={{ delay: 0.3 }}
        >
          Need help? <Link to={ROUTES.CONTACT} className={styles.helpLink}>Contact Support</Link>
        </motion.p>
      </div>
    </div>
  );
};

export default ForgotPasswordPage;
// src/features/auth/components/LoginForm/LoginForm.tsx

import React, { useState } from 'react';
import { Link } from 'react-router-dom';
import { motion } from 'framer-motion';
import { Button } from '../../../../shared/ui/Button';
import { Input } from '../../../../shared/ui/Input';
import { ROUTES } from '../../../../shared/constants/routes';
import { useAuth } from '../../hooks';
import styles from './LoginForm.module.css';

export const LoginForm: React.FC = () => {
  const { login, isLoading, error, clearError } = useAuth();
  
  const [formData, setFormData] = useState({
    email: '',
    password: '',
    rememberMe: false,
  });

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const { name, value, type, checked } = e.target;
    setFormData((prev) => ({
      ...prev,
      [name]: type === 'checkbox' ? checked : value,
    }));
    if (error) clearError();
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    await login(formData);
  };

  return (
    <motion.form 
      className={styles.form}
      onSubmit={handleSubmit}
      initial={{ opacity: 0, y: 20 }}
      animate={{ opacity: 1, y: 0 }}
      transition={{ duration: 0.3 }}
    >
      <div className={styles.header}>
        <h2 className={styles.title}>Welcome Back</h2>
        <p className={styles.subtitle}>
          Don't have an account?{' '}
          <Link to={ROUTES.REGISTER} className={styles.link}>
            Sign up
          </Link>
        </p>
      </div>

      {error && (
        <motion.div 
          className={styles.error}
          initial={{ opacity: 0, scale: 0.95 }}
          animate={{ opacity: 1, scale: 1 }}
        >
          <span className={styles.errorIcon}>⚠️</span>
          {error}
        </motion.div>
      )}

      <div className={styles.fields}>
        <Input
          label="Email Address"
          type="email"
          name="email"
          value={formData.email}
          onChange={handleChange}
          placeholder="you@example.com"
          icon={<span>📧</span>}
          required
          fullWidth
          disabled={isLoading}
        />

        <Input
          label="Password"
          type="password"
          name="password"
          value={formData.password}
          onChange={handleChange}
          placeholder="••••••••"
          required
          fullWidth
          disabled={isLoading}
        />

        <div className={styles.options}>
          <label className={styles.checkbox}>
            <input
              type="checkbox"
              name="rememberMe"
              checked={formData.rememberMe}
              onChange={handleChange}
            />
            <span className={styles.checkmark} />
            <span>Remember me</span>
          </label>
          
          <Link to={ROUTES.FORGOT_PASSWORD} className={styles.forgotLink}>
            Forgot password?
          </Link>
        </div>
      </div>

      <div className={styles.actions}>
        <Button
          type="submit"
          variant="primary"
          size="lg"
          fullWidth
          loading={isLoading}
          glow
        >
          Sign In
        </Button>
      </div>

      <div className={styles.divider}>
        <span>or continue with</span>
      </div>

      <div className={styles.social}>
        <Button variant="secondary" size="md" fullWidth disabled>
          <span>G</span> Google
        </Button>
        <Button variant="secondary" size="md" fullWidth disabled>
          <span>⌘</span> GitHub
        </Button>
      </div>
    </motion.form>
  );
};

export default LoginForm;
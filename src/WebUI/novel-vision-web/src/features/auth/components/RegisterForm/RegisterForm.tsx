// src/features/auth/components/RegisterForm/RegisterForm.tsx

import React, { useState } from 'react';
import { Link } from 'react-router-dom';
import { motion, AnimatePresence } from 'framer-motion';
import { Button } from '../../../../shared/ui/Button';
import { Input } from '../../../../shared/ui/Input';
import { ROUTES } from '../../../../shared/constants/routes';
import { useAuth } from '../../hooks';
import styles from './RegisterForm.module.css';

type Step = 1 | 2 | 3;

export const RegisterForm: React.FC = () => {
  const { register, isLoading, error, clearError } = useAuth();
  
  const [step, setStep] = useState<Step>(1);
  const [formData, setFormData] = useState({
    firstName: '',
    lastName: '',
    email: '',
    password: '',
    confirmPassword: '',
    acceptTerms: false,
  });
  const [localError, setLocalError] = useState('');

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const { name, value, type, checked } = e.target;
    setFormData((prev) => ({
      ...prev,
      [name]: type === 'checkbox' ? checked : value,
    }));
    if (error) clearError();
    setLocalError('');
  };

  const validateStep1 = (): boolean => {
    if (!formData.firstName.trim() || !formData.lastName.trim()) {
      setLocalError('Please enter your full name');
      return false;
    }
    if (!formData.email.trim() || !formData.email.includes('@')) {
      setLocalError('Please enter a valid email address');
      return false;
    }
    return true;
  };

  const validateStep2 = (): boolean => {
    if (formData.password.length < 6) {
      setLocalError('Password must be at least 6 characters');
      return false;
    }
    if (!/[A-Z]/.test(formData.password)) {
      setLocalError('Password must contain an uppercase letter');
      return false;
    }
    if (!/[0-9]/.test(formData.password)) {
      setLocalError('Password must contain a number');
      return false;
    }
    if (formData.password !== formData.confirmPassword) {
      setLocalError('Passwords do not match');
      return false;
    }
    return true;
  };

  const handleNext = () => {
    if (step === 1 && validateStep1()) {
      setStep(2);
    } else if (step === 2 && validateStep2()) {
      setStep(3);
    }
  };

  const handleBack = () => {
    setStep((prev) => (prev > 1 ? ((prev - 1) as Step) : prev));
    setLocalError('');
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    
    if (!formData.acceptTerms) {
      setLocalError('Please accept the terms and conditions');
      return;
    }

    await register({
      email: formData.email,
      password: formData.password,
      confirmPassword: formData.confirmPassword,
      firstName: formData.firstName,
      lastName: formData.lastName,
      acceptTerms: formData.acceptTerms,
    });
  };

  const displayError = localError || error;
  const passwordStrength = 
    formData.password.length >= 8 ? 'strong' : 
    formData.password.length >= 4 ? 'medium' : 'weak';

  return (
    <motion.form 
      className={styles.form}
      onSubmit={handleSubmit}
      initial={{ opacity: 0, y: 20 }}
      animate={{ opacity: 1, y: 0 }}
      transition={{ duration: 0.3 }}
    >
      <div className={styles.header}>
        <h2 className={styles.title}>Create Account</h2>
        <p className={styles.subtitle}>
          Already have an account?{' '}
          <Link to={ROUTES.LOGIN} className={styles.link}>
            Sign in
          </Link>
        </p>
      </div>

      {/* Progress Steps */}
      <div className={styles.progress}>
        {[1, 2, 3].map((s) => (
          <div 
            key={s} 
            className={`${styles.step} ${step >= s ? styles.active : ''} ${step > s ? styles.completed : ''}`}
          >
            <div className={styles.stepNumber}>
              {step > s ? '✓' : s}
            </div>
            <span className={styles.stepLabel}>
              {s === 1 && 'Profile'}
              {s === 2 && 'Security'}
              {s === 3 && 'Confirm'}
            </span>
          </div>
        ))}
      </div>

      {displayError && (
        <motion.div 
          className={styles.error}
          initial={{ opacity: 0, scale: 0.95 }}
          animate={{ opacity: 1, scale: 1 }}
        >
          <span className={styles.errorIcon}>⚠️</span>
          {displayError}
        </motion.div>
      )}

      <AnimatePresence mode="wait">
        {/* Step 1: Profile */}
        {step === 1 && (
          <motion.div
            key="step1"
            className={styles.stepContent}
            initial={{ opacity: 0, x: 20 }}
            animate={{ opacity: 1, x: 0 }}
            exit={{ opacity: 0, x: -20 }}
          >
            <div className={styles.row}>
              <Input
                label="First Name"
                name="firstName"
                value={formData.firstName}
                onChange={handleChange}
                placeholder="John"
                required
                fullWidth
              />
              <Input
                label="Last Name"
                name="lastName"
                value={formData.lastName}
                onChange={handleChange}
                placeholder="Doe"
                required
                fullWidth
              />
            </div>

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
            />
          </motion.div>
        )}

        {/* Step 2: Security */}
        {step === 2 && (
          <motion.div
            key="step2"
            className={styles.stepContent}
            initial={{ opacity: 0, x: 20 }}
            animate={{ opacity: 1, x: 0 }}
            exit={{ opacity: 0, x: -20 }}
          >
            <Input
              label="Password"
              type="password"
              name="password"
              value={formData.password}
              onChange={handleChange}
              placeholder="Min. 6 characters"
              required
              fullWidth
            />

            <div className={styles.strengthBar}>
              <div className={`${styles.strengthFill} ${styles[passwordStrength]}`} />
            </div>

            <Input
              label="Confirm Password"
              type="password"
              name="confirmPassword"
              value={formData.confirmPassword}
              onChange={handleChange}
              placeholder="Repeat password"
              required
              fullWidth
            />

            {formData.confirmPassword && (
              <span className={formData.password === formData.confirmPassword ? styles.match : styles.noMatch}>
                {formData.password === formData.confirmPassword ? '✓ Passwords match' : '✗ Passwords do not match'}
              </span>
            )}

            <div className={styles.requirements}>
              <p>Password must contain:</p>
              <ul>
                <li className={formData.password.length >= 6 ? styles.met : ''}>
                  At least 6 characters
                </li>
                <li className={/[A-Z]/.test(formData.password) ? styles.met : ''}>
                  One uppercase letter
                </li>
                <li className={/[0-9]/.test(formData.password) ? styles.met : ''}>
                  One number
                </li>
              </ul>
            </div>
          </motion.div>
        )}

        {/* Step 3: Confirm */}
        {step === 3 && (
          <motion.div
            key="step3"
            className={styles.stepContent}
            initial={{ opacity: 0, x: 20 }}
            animate={{ opacity: 1, x: 0 }}
            exit={{ opacity: 0, x: -20 }}
          >
            <div className={styles.summary}>
              <h3>Review Your Information</h3>
              <div className={styles.summaryRow}>
                <span>Name</span>
                <span>{formData.firstName} {formData.lastName}</span>
              </div>
              <div className={styles.summaryRow}>
                <span>Email</span>
                <span>{formData.email}</span>
              </div>
            </div>

            <label className={styles.termsCheckbox}>
              <input
                type="checkbox"
                name="acceptTerms"
                checked={formData.acceptTerms}
                onChange={handleChange}
              />
              <span className={styles.checkmark} />
              <span>
                I agree to the{' '}
                <Link to="/terms" className={styles.link}>Terms of Service</Link>
                {' '}and{' '}
                <Link to="/privacy" className={styles.link}>Privacy Policy</Link>
              </span>
            </label>
          </motion.div>
        )}
      </AnimatePresence>

      <div className={styles.actions}>
        {step > 1 && (
          <Button
            type="button"
            variant="secondary"
            size="lg"
            onClick={handleBack}
            disabled={isLoading}
          >
            ← Back
          </Button>
        )}
        
        {step < 3 ? (
          <Button
            type="button"
            variant="primary"
            size="lg"
            onClick={handleNext}
            fullWidth={step === 1}
          >
            Continue →
          </Button>
        ) : (
          <Button
            type="submit"
            variant="primary"
            size="lg"
            loading={isLoading}
            glow
          >
            🚀 Create Account
          </Button>
        )}
      </div>
    </motion.form>
  );
};

export default RegisterForm;
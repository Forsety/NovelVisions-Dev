// src/pages/RegisterPage.tsx
import React, { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';
import './AuthPages.css';

const RegisterPage: React.FC = () => {
  const navigate = useNavigate();
  const { register } = useAuth();
  
  const [step, setStep] = useState(1);
  const [formData, setFormData] = useState({
    email: '',
    password: '',
    confirmPassword: '',
    firstName: '',
    lastName: '',
    role: 'Reader' as 'Reader' | 'Author',
    agreeTerms: false
  });
  
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const { name, value, type, checked } = e.target;
    setFormData(prev => ({
      ...prev,
      [name]: type === 'checkbox' ? checked : value
    }));
    setError('');
  };

  const validateStep1 = () => {
    if (!formData.firstName.trim() || !formData.lastName.trim()) {
      setError('Please enter your full name');
      return false;
    }
    if (!formData.email.trim() || !formData.email.includes('@')) {
      setError('Please enter a valid email address');
      return false;
    }
    return true;
  };

  const validateStep2 = () => {
    if (formData.password.length < 8) {
      setError('Password must be at least 8 characters');
      return false;
    }
    if (formData.password !== formData.confirmPassword) {
      setError('Passwords do not match');
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
    setStep(step - 1);
    setError('');
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    
    if (!formData.agreeTerms) {
      setError('Please agree to the terms and conditions');
      return;
    }

    try {
      setLoading(true);
      setError('');
      
      await register(
        formData.email,
        formData.password,
        formData.firstName,
        formData.lastName
      );
      
      navigate('/');
    } catch (err: any) {
      setError(err.message || 'Registration failed. Please try again.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="auth-page">
      {/* Background Effects */}
      <div className="auth-background">
        <div className="bg-gradient"></div>
        <div className="bg-particles">
          {[...Array(30)].map((_, i) => (
            <div key={i} className="particle" style={{
              left: `${Math.random() * 100}%`,
              top: `${Math.random() * 100}%`,
              animationDelay: `${Math.random() * 5}s`,
              animationDuration: `${10 + Math.random() * 10}s`
            }} />
          ))}
        </div>
        <div className="bg-shapes">
          <div className="shape shape-1"></div>
          <div className="shape shape-2"></div>
          <div className="shape shape-3"></div>
        </div>
      </div>

      <div className="auth-container">
        {/* Left Panel - Branding */}
        <div className="auth-branding">
          <div className="brand-content">
            <Link to="/" className="brand-logo">
              <span className="logo-icon">🔮</span>
              <span className="logo-text">NovelVision</span>
            </Link>
            
            <h1 className="brand-title">
              Join the Future of Reading
            </h1>
            
            <p className="brand-subtitle">
              Create an account and start experiencing stories with AI-powered illustrations
            </p>

            <div className="brand-features">
              <div className="feature-item">
                <span className="feature-icon">📚</span>
                <span className="feature-text">Access thousands of books</span>
              </div>
              <div className="feature-item">
                <span className="feature-icon">🎨</span>
                <span className="feature-text">AI visualizations for every scene</span>
              </div>
              <div className="feature-item">
                <span className="feature-icon">✍️</span>
                <span className="feature-text">Publish your own stories</span>
              </div>
              <div className="feature-item">
                <span className="feature-icon">💾</span>
                <span className="feature-text">Sync across all devices</span>
              </div>
            </div>
          </div>

          <div className="brand-decoration">
            <div className="floating-book book-1">📖</div>
            <div className="floating-book book-2">📚</div>
            <div className="floating-book book-3">📕</div>
          </div>
        </div>

        {/* Right Panel - Form */}
        <div className="auth-form-container">
          <div className="form-header">
            <h2>Create Account</h2>
            <p>Already have an account? <Link to="/login">Sign in</Link></p>
          </div>

          {/* Progress Steps */}
          <div className="step-progress">
            {[1, 2, 3].map(s => (
              <div key={s} className={`step ${step >= s ? 'active' : ''} ${step > s ? 'completed' : ''}`}>
                <div className="step-number">
                  {step > s ? '✓' : s}
                </div>
                <span className="step-label">
                  {s === 1 && 'Profile'}
                  {s === 2 && 'Security'}
                  {s === 3 && 'Confirm'}
                </span>
              </div>
            ))}
          </div>

          {/* Error Message */}
          {error && (
            <div className="error-message">
              <span className="error-icon">⚠️</span>
              {error}
            </div>
          )}

          <form onSubmit={handleSubmit} className="auth-form">
            {/* Step 1: Profile */}
            {step === 1 && (
              <div className="form-step">
                <div className="form-row">
                  <div className="form-group">
                    <label htmlFor="firstName">First Name</label>
                    <input
                      type="text"
                      id="firstName"
                      name="firstName"
                      value={formData.firstName}
                      onChange={handleChange}
                      placeholder="John"
                      required
                    />
                  </div>
                  <div className="form-group">
                    <label htmlFor="lastName">Last Name</label>
                    <input
                      type="text"
                      id="lastName"
                      name="lastName"
                      value={formData.lastName}
                      onChange={handleChange}
                      placeholder="Doe"
                      required
                    />
                  </div>
                </div>

                <div className="form-group">
                  <label htmlFor="email">Email Address</label>
                  <input
                    type="email"
                    id="email"
                    name="email"
                    value={formData.email}
                    onChange={handleChange}
                    placeholder="john@example.com"
                    required
                  />
                </div>

                <div className="form-group">
                  <label>I want to</label>
                  <div className="role-options">
                    <label className={`role-option ${formData.role === 'Reader' ? 'selected' : ''}`}>
                      <input
                        type="radio"
                        name="role"
                        value="Reader"
                        checked={formData.role === 'Reader'}
                        onChange={handleChange}
                      />
                      <span className="role-icon">📖</span>
                      <span className="role-title">Read Books</span>
                      <span className="role-desc">Explore and enjoy stories</span>
                    </label>
                    <label className={`role-option ${formData.role === 'Author' ? 'selected' : ''}`}>
                      <input
                        type="radio"
                        name="role"
                        value="Author"
                        checked={formData.role === 'Author'}
                        onChange={handleChange}
                      />
                      <span className="role-icon">✍️</span>
                      <span className="role-title">Write & Publish</span>
                      <span className="role-desc">Share your stories</span>
                    </label>
                  </div>
                </div>
              </div>
            )}

            {/* Step 2: Security */}
            {step === 2 && (
              <div className="form-step">
                <div className="form-group">
                  <label htmlFor="password">Password</label>
                  <input
                    type="password"
                    id="password"
                    name="password"
                    value={formData.password}
                    onChange={handleChange}
                    placeholder="Min. 8 characters"
                    required
                  />
                  <div className="password-strength">
                    <div className={`strength-bar ${formData.password.length >= 8 ? 'strong' : formData.password.length >= 4 ? 'medium' : 'weak'}`}></div>
                  </div>
                </div>

                <div className="form-group">
                  <label htmlFor="confirmPassword">Confirm Password</label>
                  <input
                    type="password"
                    id="confirmPassword"
                    name="confirmPassword"
                    value={formData.confirmPassword}
                    onChange={handleChange}
                    placeholder="Repeat password"
                    required
                  />
                  {formData.confirmPassword && (
                    <span className={`match-indicator ${formData.password === formData.confirmPassword ? 'match' : 'no-match'}`}>
                      {formData.password === formData.confirmPassword ? '✓ Passwords match' : '✗ Passwords do not match'}
                    </span>
                  )}
                </div>

                <div className="password-requirements">
                  <p>Password must contain:</p>
                  <ul>
                    <li className={formData.password.length >= 8 ? 'met' : ''}>
                      At least 8 characters
                    </li>
                    <li className={/[A-Z]/.test(formData.password) ? 'met' : ''}>
                      One uppercase letter
                    </li>
                    <li className={/[0-9]/.test(formData.password) ? 'met' : ''}>
                      One number
                    </li>
                  </ul>
                </div>
              </div>
            )}

            {/* Step 3: Confirm */}
            {step === 3 && (
              <div className="form-step">
                <div className="summary-card">
                  <h3>Review Your Information</h3>
                  <div className="summary-row">
                    <span className="summary-label">Name</span>
                    <span className="summary-value">{formData.firstName} {formData.lastName}</span>
                  </div>
                  <div className="summary-row">
                    <span className="summary-label">Email</span>
                    <span className="summary-value">{formData.email}</span>
                  </div>
                  <div className="summary-row">
                    <span className="summary-label">Account Type</span>
                    <span className="summary-value">{formData.role}</span>
                  </div>
                </div>

                <div className="form-group checkbox-group">
                  <label className="checkbox-label">
                    <input
                      type="checkbox"
                      name="agreeTerms"
                      checked={formData.agreeTerms}
                      onChange={handleChange}
                    />
                    <span className="checkbox-custom"></span>
                    <span>
                      I agree to the <Link to="/terms">Terms of Service</Link> and{' '}
                      <Link to="/privacy">Privacy Policy</Link>
                    </span>
                  </label>
                </div>
              </div>
            )}

            {/* Form Actions */}
            <div className="form-actions">
              {step > 1 && (
                <button type="button" className="btn-secondary" onClick={handleBack}>
                  ← Back
                </button>
              )}
              
              {step < 3 ? (
                <button type="button" className="btn-primary" onClick={handleNext}>
                  Continue →
                </button>
              ) : (
                <button type="submit" className="btn-primary" disabled={loading}>
                  {loading ? (
                    <>
                      <span className="spinner"></span>
                      Creating Account...
                    </>
                  ) : (
                    <>
                      <span className="btn-icon">🚀</span>
                      Create Account
                    </>
                  )}
                </button>
              )}
            </div>
          </form>

          {/* Social Login */}
          <div className="social-login">
            <div className="divider">
              <span>or continue with</span>
            </div>
            <div className="social-buttons">
              <button type="button" className="social-btn google">
                <span className="social-icon">G</span>
                Google
              </button>
              <button type="button" className="social-btn github">
                <span className="social-icon">⌘</span>
                GitHub
              </button>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

export default RegisterPage;
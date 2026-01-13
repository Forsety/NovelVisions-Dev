// src/pages/LoginPage.tsx
import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';
import './LoginPage.css';

const LoginPage: React.FC = () => {
  const navigate = useNavigate();
  const { login, register } = useAuth();
  
  const [isLoginMode, setIsLoginMode] = useState(true);
  
  const [formData, setFormData] = useState({
    email: '',
    password: '',
    firstName: '',
    lastName: '',
    confirmPassword: ''
  });
  
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    setLoading(true);

    try {
      if (isLoginMode) {
        // Вход
        await login(formData.email, formData.password);
        navigate('/');
      } else {
        // Регистрация
        if (formData.password !== formData.confirmPassword) {
          setError('Пароли не совпадают');
          setLoading(false);
          return;
        }
        
        if (formData.password.length < 6) {
          setError('Пароль должен быть не менее 6 символов');
          setLoading(false);
          return;
        }

        if (!formData.firstName || !formData.lastName) {
          setError('Заполните имя и фамилию');
          setLoading(false);
          return;
        }
        
        await register(
          formData.email, 
          formData.password, 
          formData.firstName,
          formData.lastName
        );
        navigate('/');
      }
    } catch (err: any) {
      console.error('Auth error:', err);
      setError(err.message || 'Произошла ошибка');
    } finally {
      setLoading(false);
    }
  };

  const toggleMode = () => {
    setIsLoginMode(!isLoginMode);
    setError('');
    setFormData({
      email: '',
      password: '',
      firstName: '',
      lastName: '',
      confirmPassword: ''
    });
  };

  return (
    <div className="login-page">
      <div className="login-container">
        <div className="login-info">
          <h1>📚 Literary Realms</h1>
          <p>Добро пожаловать в мир историй!</p>
          
          <div className="features">
            <div className="feature">
              <span className="icon">📖</span>
              <div>
                <h3>Читайте</h3>
                <p>Тысячи книг в каталоге</p>
              </div>
            </div>
            <div className="feature">
              <span className="icon">✍️</span>
              <div>
                <h3>Пишите</h3>
                <p>Публикуйте свои книги</p>
              </div>
            </div>
            <div className="feature">
              <span className="icon">🎨</span>
              <div>
                <h3>Визуализируйте</h3>
                <p>AI создаст иллюстрации</p>
              </div>
            </div>
          </div>

          <div className="test-credentials">
            <h4>ℹ️ Информация о системе:</h4>
            <div className="credential">
              <strong>API Endpoint:</strong>
              <span>localhost:5001</span>
            </div>
            <div className="credential">
              <strong>Роли:</strong>
              <span>Reader, Author, Admin</span>
            </div>
            <div className="credential">
              <strong>Статус:</strong>
              <span style={{ color: '#4caf50' }}>● Online</span>
            </div>
          </div>
        </div>

        <div className="login-form-container">
          <div className="form-header">
            <h2>{isLoginMode ? 'Вход в систему' : 'Регистрация'}</h2>
            <p>
              {isLoginMode 
                ? 'Войдите в свой аккаунт' 
                : 'Создайте новый аккаунт'}
            </p>
          </div>

          {error && (
            <div className="error-message">
              ⚠️ {error}
            </div>
          )}

          <form onSubmit={handleSubmit} className="login-form">
            {!isLoginMode && (
              <>
                <div className="form-group">
                  <label>Имя *</label>
                  <input
                    type="text"
                    value={formData.firstName}
                    onChange={(e) => setFormData({...formData, firstName: e.target.value})}
                    placeholder="Ваше имя"
                    required={!isLoginMode}
                  />
                </div>

                <div className="form-group">
                  <label>Фамилия *</label>
                  <input
                    type="text"
                    value={formData.lastName}
                    onChange={(e) => setFormData({...formData, lastName: e.target.value})}
                    placeholder="Ваша фамилия"
                    required={!isLoginMode}
                  />
                </div>
              </>
            )}

            <div className="form-group">
              <label>Email *</label>
              <input
                type="email"
                value={formData.email}
                onChange={(e) => setFormData({...formData, email: e.target.value})}
                placeholder="example@email.com"
                required
              />
            </div>

            <div className="form-group">
              <label>Пароль *</label>
              <input
                type="password"
                value={formData.password}
                onChange={(e) => setFormData({...formData, password: e.target.value})}
                placeholder="••••••••"
                required
              />
              {!isLoginMode && (
                <small style={{ color: '#7f8c8d', fontSize: '12px' }}>
                  Минимум 6 символов, должен содержать цифры, заглавные и строчные буквы
                </small>
              )}
            </div>

            {!isLoginMode && (
              <div className="form-group">
                <label>Подтвердите пароль *</label>
                <input
                  type="password"
                  value={formData.confirmPassword}
                  onChange={(e) => setFormData({...formData, confirmPassword: e.target.value})}
                  placeholder="••••••••"
                  required={!isLoginMode}
                />
              </div>
            )}

            <button 
              type="submit" 
              className="submit-btn"
              disabled={loading}
            >
              {loading 
                ? 'Загрузка...' 
                : (isLoginMode ? 'Войти' : 'Зарегистрироваться')
              }
            </button>
          </form>

          <div className="form-footer">
            <p>
              {isLoginMode ? 'Нет аккаунта?' : 'Уже есть аккаунт?'}
              <button onClick={toggleMode} className="toggle-btn">
                {isLoginMode ? 'Зарегистрироваться' : 'Войти'}
              </button>
            </p>
            
            {isLoginMode && (
              <a href="#" className="forgot-link" onClick={(e) => {
                e.preventDefault();
                alert('Функция восстановления пароля будет доступна позже');
              }}>
                Забыли пароль?
              </a>
            )}
          </div>

          <div className="divider">
            <span>ИЛИ</span>
          </div>

          <button 
            className="guest-btn"
            onClick={() => navigate('/')}
          >
            👁️ Продолжить как гость
          </button>
        </div>
      </div>
    </div>
  );
};

export default LoginPage;
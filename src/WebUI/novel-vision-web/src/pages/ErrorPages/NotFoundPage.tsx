// src/pages/ErrorPages/NotFoundPage.tsx
// 404 Not Found page with animated illustration

import React from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { motion } from 'framer-motion';
import { Button } from '../../shared/ui/Button';
import { ROUTES } from '../../shared/constants/routes';
import styles from './NotFoundPage.module.css';

const NotFoundPage: React.FC = () => {
  const navigate = useNavigate();

  return (
    <div className={styles.page}>
      <div className={styles.container}>
        {/* Animated 404 */}
        <motion.div
          className={styles.errorCode}
          initial={{ opacity: 0, scale: 0.8 }}
          animate={{ opacity: 1, scale: 1 }}
          transition={{ duration: 0.5, ease: 'easeOut' }}
        >
          <motion.span
            animate={{ 
              rotateY: [0, 360],
              color: ['#a78bfa', '#6366f1', '#ec4899', '#a78bfa'],
            }}
            transition={{ 
              duration: 4, 
              repeat: Infinity, 
              ease: 'linear',
            }}
          >
            4
          </motion.span>
          <motion.span
            className={styles.zeroIcon}
            animate={{ 
              rotate: [0, 10, -10, 0],
              scale: [1, 1.1, 1],
            }}
            transition={{ 
              duration: 2, 
              repeat: Infinity,
              ease: 'easeInOut',
            }}
          >
            📚
          </motion.span>
          <motion.span
            animate={{ 
              rotateY: [0, 360],
              color: ['#ec4899', '#a78bfa', '#6366f1', '#ec4899'],
            }}
            transition={{ 
              duration: 4, 
              repeat: Infinity, 
              ease: 'linear',
              delay: 0.5,
            }}
          >
            4
          </motion.span>
        </motion.div>

        {/* Message */}
        <motion.div
          className={styles.content}
          initial={{ opacity: 0, y: 20 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ delay: 0.3 }}
        >
          <h1 className={styles.title}>Page Not Found</h1>
          <p className={styles.message}>
            Oops! It seems the page you're looking for has wandered off into 
            another story. Let's get you back to familiar chapters.
          </p>

          {/* Actions */}
          <div className={styles.actions}>
            <Button
              variant="primary"
              size="lg"
              glow
              onClick={() => navigate(ROUTES.HOME)}
            >
              🏠 Go Home
            </Button>
            <Button
              variant="outline"
              size="lg"
              onClick={() => navigate(-1)}
            >
              ← Go Back
            </Button>
          </div>

          {/* Helpful Links */}
          <div className={styles.links}>
            <span className={styles.linksLabel}>Or explore:</span>
            <Link to={ROUTES.CATALOG} className={styles.link}>
              📖 Library
            </Link>
            <Link to={ROUTES.GUTENBERG} className={styles.link}>
              📚 Gutenberg
            </Link>
          </div>
        </motion.div>

        {/* Floating Books Animation */}
        <div className={styles.floatingBooks}>
          {['📕', '📗', '📘', '📙', '📓'].map((book, i) => (
            <motion.span
              key={i}
              className={styles.floatingBook}
              initial={{ 
                x: Math.random() * 200 - 100,
                y: Math.random() * 200 - 100,
                opacity: 0,
              }}
              animate={{ 
                y: [0, -20, 0],
                opacity: [0.3, 0.6, 0.3],
                rotate: [0, 10, -10, 0],
              }}
              transition={{ 
                duration: 3 + Math.random() * 2,
                repeat: Infinity,
                delay: i * 0.5,
                ease: 'easeInOut',
              }}
              style={{
                left: `${10 + i * 20}%`,
                top: `${20 + Math.random() * 60}%`,
              }}
            >
              {book}
            </motion.span>
          ))}
        </div>
      </div>
    </div>
  );
};

export default NotFoundPage;
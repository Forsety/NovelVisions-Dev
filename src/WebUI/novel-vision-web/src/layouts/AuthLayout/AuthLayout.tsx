// src/layouts/AuthLayout/AuthLayout.tsx

import React from 'react';
import { Link, Outlet } from 'react-router-dom';
import { motion } from 'framer-motion';
import styles from './AuthLayout.module.css';

export const AuthLayout: React.FC = () => {
  return (
    <div className={styles.page}>
      {/* Background Effects */}
      <div className={styles.background}>
        <div className={styles.gradient} />
        <div className={styles.shapes}>
          <div className={`${styles.shape} ${styles.shape1}`} />
          <div className={`${styles.shape} ${styles.shape2}`} />
          <div className={`${styles.shape} ${styles.shape3}`} />
        </div>
        <div className={styles.particles}>
          {[...Array(20)].map((_, i) => (
            <div
              key={i}
              className={styles.particle}
              style={{
                left: `${Math.random() * 100}%`,
                animationDelay: `${Math.random() * 10}s`,
                animationDuration: `${15 + Math.random() * 10}s`,
              }}
            />
          ))}
        </div>
      </div>

      {/* Main Container */}
      <div className={styles.container}>
        {/* Branding Panel */}
        <motion.div 
          className={styles.branding}
          initial={{ opacity: 0, x: -30 }}
          animate={{ opacity: 1, x: 0 }}
          transition={{ duration: 0.5 }}
        >
          <Link to="/" className={styles.logo}>
            <span className={styles.logoIcon}>🔮</span>
            <span className={styles.logoText}>NovelVision</span>
          </Link>

          <div className={styles.brandContent}>
            <h1 className={styles.brandTitle}>
              Experience Stories <br />
              <span className={styles.highlight}>Like Never Before</span>
            </h1>
            <p className={styles.brandSubtitle}>
              AI-powered visualizations bring your favorite books to life
              with stunning illustrations and immersive experiences.
            </p>

            <div className={styles.features}>
              <div className={styles.feature}>
                <span className={styles.featureIcon}>🎨</span>
                <span>AI Visualizations</span>
              </div>
              <div className={styles.feature}>
                <span className={styles.featureIcon}>📚</span>
                <span>Vast Library</span>
              </div>
              <div className={styles.feature}>
                <span className={styles.featureIcon}>✍️</span>
                <span>Publish Stories</span>
              </div>
            </div>
          </div>

          <div className={styles.decoration}>
            <span className={`${styles.floatingBook} ${styles.book1}`}>📖</span>
            <span className={`${styles.floatingBook} ${styles.book2}`}>📚</span>
            <span className={`${styles.floatingBook} ${styles.book3}`}>✨</span>
          </div>
        </motion.div>

        {/* Form Panel */}
        <motion.div 
          className={styles.formPanel}
          initial={{ opacity: 0, x: 30 }}
          animate={{ opacity: 1, x: 0 }}
          transition={{ duration: 0.5, delay: 0.1 }}
        >
          <Outlet />
        </motion.div>
      </div>
    </div>
  );
};

export default AuthLayout;
// src/pages/HomePage/HomePage.tsx
// ╔══════════════════════════════════════════════════════════════════════════════════════════════════╗
// ║   NOVELVISION HOME PAGE v3.0                                                                     ║
// ║   Premium Library Landing - Diploma Quality Implementation                                        ║
// ╚══════════════════════════════════════════════════════════════════════════════════════════════════╝

import React, { useState, useEffect, useRef } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { motion, useScroll, useTransform, useInView, AnimatePresence, type Variants } from 'framer-motion';
import { ROUTES } from '../../shared/constants/routes';
import { useAuthStore } from '../../store';
import styles from './HomePage.module.css';

// ═══════════════════════════════════════════════════════════════════════════════════════════════════
// TYPES
// ═══════════════════════════════════════════════════════════════════════════════════════════════════

interface Book {
  id: string;
  title: string;
  author: string;
  cover?: string;
  rating?: number;
  genre?: string;
}

interface Feature {
  icon: string;
  title: string;
  description: string;
  color: string;
}

interface Genre {
  id: string;
  name: string;
  icon: string;
  count: string;
  gradient: string;
}

interface Testimonial {
  id: string;
  name: string;
  role: string;
  avatar: string;
  quote: string;
  rating: number;
}

interface Stat {
  icon: string;
  value: string;
  label: string;
  suffix?: string;
}

// ═══════════════════════════════════════════════════════════════════════════════════════════════════
// DATA
// ═══════════════════════════════════════════════════════════════════════════════════════════════════

const STATS: Stat[] = [
  { icon: '📚', value: '70', label: 'Books', suffix: 'K+' },
  { icon: '🎨', value: '1', label: 'AI Illustrations', suffix: 'M+' },
  { icon: '👥', value: '50', label: 'Active Readers', suffix: 'K+' },
  { icon: '✍️', value: '10', label: 'Authors', suffix: 'K+' },
];

const FEATURES: Feature[] = [
  {
    icon: '🎨',
    title: 'AI Illustrations',
    description: 'Every scene brought to life with stunning AI-generated artwork. Choose from multiple styles including realistic, anime, oil painting, and more.',
    color: 'gold',
  },
  {
    icon: '🎵',
    title: 'Ambient Soundscapes',
    description: 'Immersive musical accompaniment that adapts to the mood of each chapter, enhancing your emotional connection to the story.',
    color: 'burgundy',
  },
  {
    icon: '📱',
    title: 'Cross-Device Sync',
    description: 'Seamlessly continue reading across all your devices. Your progress, bookmarks, and preferences sync automatically.',
    color: 'emerald',
  },
  {
    icon: '✨',
    title: 'Personalized Experience',
    description: 'Customize your reading with multiple themes, fonts, and layouts. Let AI learn your preferences for better recommendations.',
    color: 'gold',
  },
  {
    icon: '🔖',
    title: 'Smart Bookmarks',
    description: 'Create rich bookmarks with notes, highlights, and AI-generated summaries. Never lose track of important passages.',
    color: 'burgundy',
  },
  {
    icon: '📊',
    title: 'Reading Analytics',
    description: 'Track your reading habits, set goals, and celebrate achievements. Understand your reading patterns with detailed insights.',
    color: 'emerald',
  },
];

const GENRES: Genre[] = [
  { id: 'fiction', name: 'Fiction', icon: '📖', count: '12,450', gradient: 'linear-gradient(135deg, #d4a574, #c17f59)' },
  { id: 'fantasy', name: 'Fantasy', icon: '🐉', count: '8,320', gradient: 'linear-gradient(135deg, #9b8bd8, #7b6bc4)' },
  { id: 'mystery', name: 'Mystery', icon: '🔍', count: '6,890', gradient: 'linear-gradient(135deg, #7eb8d8, #5a9fc4)' },
  { id: 'romance', name: 'Romance', icon: '💕', count: '9,120', gradient: 'linear-gradient(135deg, #f4a9ba, #df4d71)' },
  { id: 'scifi', name: 'Sci-Fi', icon: '🚀', count: '5,670', gradient: 'linear-gradient(135deg, #60a5fa, #3b82f6)' },
  { id: 'history', name: 'History', icon: '🏛️', count: '4,230', gradient: 'linear-gradient(135deg, #d4a574, #8b5a3c)' },
  { id: 'horror', name: 'Horror', icon: '👻', count: '3,450', gradient: 'linear-gradient(135deg, #6b5c54, #3a302a)' },
  { id: 'philosophy', name: 'Philosophy', icon: '🤔', count: '2,180', gradient: 'linear-gradient(135deg, #6db889, #4a9c6a)' },
];

const FEATURED_BOOKS: Book[] = [
  { id: '1', title: 'Pride and Prejudice', author: 'Jane Austen', rating: 4.8, genre: 'Classic' },
  { id: '2', title: 'The Great Gatsby', author: 'F. Scott Fitzgerald', rating: 4.7, genre: 'Classic' },
  { id: '3', title: 'Moby Dick', author: 'Herman Melville', rating: 4.5, genre: 'Adventure' },
  { id: '4', title: 'War and Peace', author: 'Leo Tolstoy', rating: 4.9, genre: 'Historical' },
  { id: '5', title: '1984', author: 'George Orwell', rating: 4.8, genre: 'Dystopian' },
  { id: '6', title: 'Jane Eyre', author: 'Charlotte Brontë', rating: 4.6, genre: 'Romance' },
];

const TESTIMONIALS: Testimonial[] = [
  {
    id: '1',
    name: 'Sarah Mitchell',
    role: 'Book Blogger',
    avatar: '👩‍💼',
    quote: 'NovelVision has completely transformed how I experience literature. The AI illustrations bring every scene to life in ways I never imagined possible.',
    rating: 5,
  },
  {
    id: '2',
    name: 'James Chen',
    role: 'Literature Professor',
    avatar: '👨‍🏫',
    quote: 'As an educator, I find the visual interpretations invaluable for helping students engage with classic texts. A revolutionary tool for modern learning.',
    rating: 5,
  },
  {
    id: '3',
    name: 'Emma Rodriguez',
    role: 'Avid Reader',
    avatar: '👩‍🎨',
    quote: 'The ambient soundscapes create such an immersive atmosphere. I feel like I\'m living inside the stories. Absolutely magical experience!',
    rating: 5,
  },
];

// ═══════════════════════════════════════════════════════════════════════════════════════════════════
// ANIMATION VARIANTS
// ═══════════════════════════════════════════════════════════════════════════════════════════════════

const fadeInUp: Variants = {
  hidden: { opacity: 0, y: 30 },
  visible: { 
    opacity: 1, 
    y: 0,
    transition: { duration: 0.6 }
  }
};

const fadeInDown: Variants = {
  hidden: { opacity: 0, y: -30 },
  visible: { 
    opacity: 1, 
    y: 0,
    transition: { duration: 0.6 }
  }
};

const fadeInLeft: Variants = {
  hidden: { opacity: 0, x: -40 },
  visible: { 
    opacity: 1, 
    x: 0,
    transition: { duration: 0.6 }
  }
};

const fadeInRight: Variants = {
  hidden: { opacity: 0, x: 40 },
  visible: { 
    opacity: 1, 
    x: 0,
    transition: { duration: 0.6 }
  }
};

const staggerContainer: Variants = {
  hidden: { opacity: 0 },
  visible: {
    opacity: 1,
    transition: {
      staggerChildren: 0.1,
      delayChildren: 0.1
    }
  }
};

const scaleIn: Variants = {
  hidden: { opacity: 0, scale: 0.9 },
  visible: { 
    opacity: 1, 
    scale: 1,
    transition: { duration: 0.5 }
  }
};

const bookHover: Variants = {
  rest: { 
    y: 0, 
    rotateY: 0,
    boxShadow: '0 10px 30px rgba(0,0,0,0.3)',
    transition: { duration: 0.4 }
  },
  hover: { 
    y: -16, 
    rotateY: -8,
    boxShadow: '0 30px 60px rgba(0,0,0,0.4), 0 0 40px rgba(212,165,116,0.15)',
    transition: { duration: 0.4 }
  }
};

// ═══════════════════════════════════════════════════════════════════════════════════════════════════
// HELPER COMPONENTS
// ═══════════════════════════════════════════════════════════════════════════════════════════════════

// Animated Counter
const AnimatedCounter: React.FC<{ value: string; suffix?: string }> = ({ value, suffix }) => {
  const [count, setCount] = useState(0);
  const ref = useRef<HTMLSpanElement>(null);
  const isInView = useInView(ref, { once: true, margin: "-100px" });
  const targetValue = parseInt(value);

  useEffect(() => {
    if (isInView) {
      const duration = 2000;
      const steps = 60;
      const increment = targetValue / steps;
      let current = 0;
      
      const timer = setInterval(() => {
        current += increment;
        if (current >= targetValue) {
          setCount(targetValue);
          clearInterval(timer);
        } else {
          setCount(Math.floor(current));
        }
      }, duration / steps);

      return () => clearInterval(timer);
    }
  }, [isInView, targetValue]);

  return (
    <span ref={ref}>
      {count}
      {suffix}
    </span>
  );
};

// Star Rating
const StarRating: React.FC<{ rating: number }> = ({ rating }) => {
  return (
    <div className={styles.starRating}>
      {[1, 2, 3, 4, 5].map((star) => (
        <span
          key={star}
          className={`${styles.star} ${star <= rating ? styles.filled : ''}`}
        >
          ★
        </span>
      ))}
    </div>
  );
};

// Section Header
const SectionHeader: React.FC<{
  label?: string;
  title: string;
  subtitle?: string;
  align?: 'left' | 'center';
}> = ({ label, title, subtitle, align = 'center' }) => {
  const ref = useRef(null);
  const isInView = useInView(ref, { once: true, margin: "-100px" });

  return (
    <motion.div
      ref={ref}
      className={`${styles.sectionHeader} ${align === 'left' ? styles.alignLeft : ''}`}
      initial="hidden"
      animate={isInView ? "visible" : "hidden"}
      variants={staggerContainer}
    >
      {label && (
        <motion.span className={styles.sectionLabel} variants={fadeInUp}>
          {label}
        </motion.span>
      )}
      <motion.h2 className={styles.sectionTitle} variants={fadeInUp}>
        {title}
      </motion.h2>
      {subtitle && (
        <motion.p className={styles.sectionSubtitle} variants={fadeInUp}>
          {subtitle}
        </motion.p>
      )}
    </motion.div>
  );
};

// ═══════════════════════════════════════════════════════════════════════════════════════════════════
// HERO SECTION
// ═══════════════════════════════════════════════════════════════════════════════════════════════════

const HeroSection: React.FC = () => {
  const navigate = useNavigate();
  const { isAuthenticated } = useAuthStore();
  const { scrollY } = useScroll();
  
  const y = useTransform(scrollY, [0, 500], [0, 150]);
  const opacity = useTransform(scrollY, [0, 300], [1, 0]);

  return (
    <section className={styles.hero}>
      {/* Background Effects */}
      <div className={styles.heroBackground}>
        <div className={styles.heroGradient} />
        <div className={styles.heroOrbs}>
          <motion.div 
            className={styles.orb1}
            animate={{ 
              x: [0, 30, 0],
              y: [0, -20, 0],
              scale: [1, 1.1, 1]
            }}
            transition={{ duration: 20, repeat: Infinity, ease: "easeInOut" }}
          />
          <motion.div 
            className={styles.orb2}
            animate={{ 
              x: [0, -40, 0],
              y: [0, 30, 0],
              scale: [1, 1.15, 1]
            }}
            transition={{ duration: 25, repeat: Infinity, ease: "easeInOut" }}
          />
          <motion.div 
            className={styles.orb3}
            animate={{ 
              x: [0, 25, 0],
              y: [0, 40, 0],
              scale: [1, 0.9, 1]
            }}
            transition={{ duration: 18, repeat: Infinity, ease: "easeInOut" }}
          />
        </div>
        <div className={styles.heroPattern} />
        <div className={styles.heroNoise} />
      </div>

      {/* Content */}
      <motion.div 
        className={styles.heroContent}
        style={{ y, opacity }}
      >
        <motion.div
          initial="hidden"
          animate="visible"
          variants={staggerContainer}
          className={styles.heroInner}
        >
          {/* Badge */}
          <motion.div className={styles.heroBadge} variants={fadeInDown}>
            <span className={styles.badgeIcon}>✨</span>
            <span>AI-Powered Reading Experience</span>
            <span className={styles.badgeNew}>NEW</span>
          </motion.div>

          {/* Title */}
          <motion.h1 className={styles.heroTitle} variants={fadeInUp}>
            Where Stories
            <br />
            <span className={styles.heroGradientText}>Come to Life</span>
          </motion.h1>

          {/* Subtitle */}
          <motion.p className={styles.heroSubtitle} variants={fadeInUp}>
            Immerse yourself in literature like never before. NovelVision transforms 
            classic and contemporary works with AI-generated illustrations, ambient 
            soundscapes, and personalized reading experiences.
          </motion.p>

          {/* CTA Buttons */}
          <motion.div className={styles.heroActions} variants={fadeInUp}>
            <button
              className={styles.primaryBtn}
              onClick={() => navigate(isAuthenticated ? ROUTES.CATALOG : ROUTES.REGISTER)}
            >
              <span className={styles.btnIcon}>📚</span>
              <span>{isAuthenticated ? 'Browse Library' : 'Start Reading Free'}</span>
              <span className={styles.btnArrow}>→</span>
            </button>
            <button
              className={styles.secondaryBtn}
              onClick={() => navigate(ROUTES.CATALOG)}
            >
              <span className={styles.btnIcon}>🎨</span>
              <span>Explore Features</span>
            </button>
          </motion.div>

          {/* Stats */}
          <motion.div className={styles.heroStats} variants={fadeInUp}>
            {STATS.map((stat, index) => (
              <div key={stat.label} className={styles.statItem}>
                <span className={styles.statIcon}>{stat.icon}</span>
                <span className={styles.statValue}>
                  <AnimatedCounter value={stat.value} suffix={stat.suffix} />
                </span>
                <span className={styles.statLabel}>{stat.label}</span>
              </div>
            ))}
          </motion.div>
        </motion.div>
      </motion.div>

      {/* Scroll Indicator */}
      <motion.div 
        className={styles.scrollIndicator}
        initial={{ opacity: 0 }}
        animate={{ opacity: 1 }}
        transition={{ delay: 1.5 }}
      >
        <span className={styles.scrollText}>Scroll to explore</span>
        <div className={styles.scrollMouse}>
          <motion.div 
            className={styles.scrollWheel}
            animate={{ y: [0, 8, 0] }}
            transition={{ duration: 1.5, repeat: Infinity }}
          />
        </div>
      </motion.div>
    </section>
  );
};

// ═══════════════════════════════════════════════════════════════════════════════════════════════════
// FEATURES SECTION
// ═══════════════════════════════════════════════════════════════════════════════════════════════════

const FeaturesSection: React.FC = () => {
  const ref = useRef(null);
  const isInView = useInView(ref, { once: true, margin: "-100px" });

  return (
    <section className={styles.features} ref={ref}>
      <div className={styles.featuresInner}>
        <SectionHeader
          label="Why NovelVision"
          title="A Revolutionary Reading Experience"
          subtitle="Discover features designed to transform how you engage with literature"
        />

        <motion.div
          className={styles.featuresGrid}
          initial="hidden"
          animate={isInView ? "visible" : "hidden"}
          variants={staggerContainer}
        >
          {FEATURES.map((feature, index) => (
            <motion.div
              key={feature.title}
              className={`${styles.featureCard} ${styles[feature.color]}`}
              variants={fadeInUp}
              whileHover={{ y: -8, transition: { duration: 0.3 } }}
            >
              <div className={styles.featureIconWrapper}>
                <span className={styles.featureIcon}>{feature.icon}</span>
                <div className={styles.featureIconGlow} />
              </div>
              <h3 className={styles.featureTitle}>{feature.title}</h3>
              <p className={styles.featureDescription}>{feature.description}</p>
              <div className={styles.featureHoverEffect} />
            </motion.div>
          ))}
        </motion.div>
      </div>
    </section>
  );
};

// ═══════════════════════════════════════════════════════════════════════════════════════════════════
// BOOKSHELF SECTION
// ═══════════════════════════════════════════════════════════════════════════════════════════════════

const BookshelfSection: React.FC = () => {
  const navigate = useNavigate();
  const ref = useRef(null);
  const isInView = useInView(ref, { once: true, margin: "-100px" });

  return (
    <section className={styles.bookshelf} ref={ref}>
      <div className={styles.bookshelfInner}>
        {/* Header */}
        <div className={styles.bookshelfHeader}>
          <div className={styles.bookshelfTitleGroup}>
            <span className={styles.sectionLabel}>Featured Collection</span>
            <h2 className={styles.sectionTitle}>Staff Picks This Week</h2>
            <p className={styles.sectionSubtitle}>
              Handpicked literary treasures enhanced with stunning AI visualizations
            </p>
          </div>
          <Link to={ROUTES.CATALOG} className={styles.viewAllLink}>
            <span>Browse All Books</span>
            <span className={styles.viewAllArrow}>→</span>
          </Link>
        </div>

        {/* Books Grid */}
        <motion.div
          className={styles.booksGrid}
          initial="hidden"
          animate={isInView ? "visible" : "hidden"}
          variants={staggerContainer}
        >
          {FEATURED_BOOKS.map((book, index) => (
            <motion.div
              key={book.id}
              className={styles.bookItem}
              variants={fadeInUp}
              custom={index}
            >
              <motion.div
                className={styles.bookCard}
                initial="rest"
                whileHover="hover"
                animate="rest"
                variants={bookHover}
                onClick={() => navigate(ROUTES.BOOK.replace(':id', book.id))}
                style={{ transformStyle: 'preserve-3d' }}
              >
                <div className={styles.bookSpine} />
                <div className={styles.bookCover}>
                  {book.cover ? (
                    <img src={book.cover} alt={book.title} />
                  ) : (
                    <div className={styles.bookPlaceholder}>
                      <span className={styles.bookPlaceholderIcon}>📖</span>
                      <span className={styles.bookPlaceholderTitle}>{book.title}</span>
                      <span className={styles.bookPlaceholderAuthor}>{book.author}</span>
                    </div>
                  )}
                  <div className={styles.bookOverlay}>
                    <span className={styles.bookGenre}>{book.genre}</span>
                    <div className={styles.bookRating}>
                      <span>★</span>
                      <span>{book.rating}</span>
                    </div>
                  </div>
                </div>
                <div className={styles.bookShadow} />
              </motion.div>
              <div className={styles.bookInfo}>
                <h4 className={styles.bookTitle}>{book.title}</h4>
                <p className={styles.bookAuthor}>{book.author}</p>
              </div>
            </motion.div>
          ))}
        </motion.div>

        {/* Shelf */}
        <div className={styles.shelf}>
          <div className={styles.shelfTop} />
          <div className={styles.shelfFront} />
          <div className={styles.shelfShadow} />
        </div>
      </div>
    </section>
  );
};

// ═══════════════════════════════════════════════════════════════════════════════════════════════════
// GENRES SECTION
// ═══════════════════════════════════════════════════════════════════════════════════════════════════

const GenresSection: React.FC = () => {
  const navigate = useNavigate();
  const ref = useRef(null);
  const isInView = useInView(ref, { once: true, margin: "-100px" });

  return (
    <section className={styles.genres} ref={ref}>
      <div className={styles.genresInner}>
        <SectionHeader
          label="Browse by Genre"
          title="Find Your Next Adventure"
          subtitle="Explore our vast collection organized by your favorite genres"
        />

        <motion.div
          className={styles.genresGrid}
          initial="hidden"
          animate={isInView ? "visible" : "hidden"}
          variants={staggerContainer}
        >
          {GENRES.map((genre, index) => (
            <motion.div
              key={genre.id}
              className={styles.genreCard}
              variants={fadeInUp}
              whileHover={{ y: -6, scale: 1.02 }}
              onClick={() => navigate(`${ROUTES.CATALOG}?genre=${genre.id}`)}
              style={{ '--genre-gradient': genre.gradient } as React.CSSProperties}
            >
              <div className={styles.genreIconWrapper}>
                <span className={styles.genreIcon}>{genre.icon}</span>
              </div>
              <h3 className={styles.genreName}>{genre.name}</h3>
              <span className={styles.genreCount}>{genre.count} books</span>
              <div className={styles.genreGlow} />
            </motion.div>
          ))}
        </motion.div>
      </div>
    </section>
  );
};

// ═══════════════════════════════════════════════════════════════════════════════════════════════════
// TESTIMONIALS SECTION
// ═══════════════════════════════════════════════════════════════════════════════════════════════════

const TestimonialsSection: React.FC = () => {
  const [activeIndex, setActiveIndex] = useState(0);
  const ref = useRef(null);
  const isInView = useInView(ref, { once: true, margin: "-100px" });

  useEffect(() => {
    const interval = setInterval(() => {
      setActiveIndex((prev) => (prev + 1) % TESTIMONIALS.length);
    }, 5000);
    return () => clearInterval(interval);
  }, []);

  return (
    <section className={styles.testimonials} ref={ref}>
      <div className={styles.testimonialsInner}>
        <SectionHeader
          label="What Readers Say"
          title="Loved by Book Enthusiasts"
          subtitle="Join thousands of readers who have transformed their reading experience"
        />

        <motion.div
          className={styles.testimonialsContent}
          initial="hidden"
          animate={isInView ? "visible" : "hidden"}
          variants={fadeInUp}
        >
          <div className={styles.testimonialCards}>
            <AnimatePresence mode="wait">
              <motion.div
                key={activeIndex}
                className={styles.testimonialCard}
                initial={{ opacity: 0, x: 50 }}
                animate={{ opacity: 1, x: 0 }}
                exit={{ opacity: 0, x: -50 }}
                transition={{ duration: 0.5 }}
              >
                <div className={styles.testimonialQuote}>
                  <span className={styles.quoteIcon}>"</span>
                  <p>{TESTIMONIALS[activeIndex].quote}</p>
                </div>
                <div className={styles.testimonialAuthor}>
                  <div className={styles.testimonialAvatar}>
                    {TESTIMONIALS[activeIndex].avatar}
                  </div>
                  <div className={styles.testimonialInfo}>
                    <span className={styles.testimonialName}>
                      {TESTIMONIALS[activeIndex].name}
                    </span>
                    <span className={styles.testimonialRole}>
                      {TESTIMONIALS[activeIndex].role}
                    </span>
                  </div>
                  <StarRating rating={TESTIMONIALS[activeIndex].rating} />
                </div>
              </motion.div>
            </AnimatePresence>
          </div>

          {/* Dots */}
          <div className={styles.testimonialDots}>
            {TESTIMONIALS.map((_, index) => (
              <button
                key={index}
                className={`${styles.testimonialDot} ${index === activeIndex ? styles.active : ''}`}
                onClick={() => setActiveIndex(index)}
                aria-label={`Go to testimonial ${index + 1}`}
              />
            ))}
          </div>
        </motion.div>
      </div>
    </section>
  );
};

// ═══════════════════════════════════════════════════════════════════════════════════════════════════
// HOW IT WORKS SECTION
// ═══════════════════════════════════════════════════════════════════════════════════════════════════

const HowItWorksSection: React.FC = () => {
  const ref = useRef(null);
  const isInView = useInView(ref, { once: true, margin: "-100px" });

  const steps = [
    {
      number: '01',
      icon: '📚',
      title: 'Choose Your Book',
      description: 'Browse our extensive library of classic and contemporary literature'
    },
    {
      number: '02',
      icon: '🎨',
      title: 'Select Art Style',
      description: 'Pick from realistic, anime, oil painting, or other visualization styles'
    },
    {
      number: '03',
      icon: '✨',
      title: 'Start Reading',
      description: 'Experience stories with AI illustrations generated as you read'
    },
    {
      number: '04',
      icon: '🎵',
      title: 'Enjoy Ambiance',
      description: 'Let adaptive soundscapes enhance your emotional journey'
    },
  ];

  return (
    <section className={styles.howItWorks} ref={ref}>
      <div className={styles.howItWorksInner}>
        <SectionHeader
          label="Getting Started"
          title="How NovelVision Works"
          subtitle="Your journey to immersive reading begins in four simple steps"
        />

        <motion.div
          className={styles.stepsGrid}
          initial="hidden"
          animate={isInView ? "visible" : "hidden"}
          variants={staggerContainer}
        >
          {steps.map((step, index) => (
            <motion.div
              key={step.number}
              className={styles.stepCard}
              variants={fadeInUp}
            >
              <div className={styles.stepNumber}>{step.number}</div>
              <div className={styles.stepIconWrapper}>
                <span className={styles.stepIcon}>{step.icon}</span>
              </div>
              <h3 className={styles.stepTitle}>{step.title}</h3>
              <p className={styles.stepDescription}>{step.description}</p>
              {index < steps.length - 1 && (
                <div className={styles.stepConnector}>
                  <span>→</span>
                </div>
              )}
            </motion.div>
          ))}
        </motion.div>
      </div>
    </section>
  );
};

// ═══════════════════════════════════════════════════════════════════════════════════════════════════
// CTA SECTION
// ═══════════════════════════════════════════════════════════════════════════════════════════════════

const CTASection: React.FC = () => {
  const navigate = useNavigate();
  const { isAuthenticated } = useAuthStore();
  const ref = useRef(null);
  const isInView = useInView(ref, { once: true, margin: "-100px" });

  return (
    <section className={styles.cta} ref={ref}>
      <div className={styles.ctaBackground}>
        <div className={styles.ctaOrb1} />
        <div className={styles.ctaOrb2} />
      </div>
      
      <motion.div
        className={styles.ctaContent}
        initial="hidden"
        animate={isInView ? "visible" : "hidden"}
        variants={staggerContainer}
      >
        <motion.div className={styles.ctaCard} variants={scaleIn}>
          <motion.span className={styles.ctaIcon} variants={fadeInUp}>
            📚
          </motion.span>
          <motion.h2 className={styles.ctaTitle} variants={fadeInUp}>
            Ready to Transform Your Reading?
          </motion.h2>
          <motion.p className={styles.ctaDescription} variants={fadeInUp}>
            Join thousands of readers who have discovered a new way to experience literature.
            Create your free account and start exploring today.
          </motion.p>
          <motion.div className={styles.ctaActions} variants={fadeInUp}>
            <button
              className={styles.ctaPrimaryBtn}
              onClick={() => navigate(isAuthenticated ? ROUTES.CATALOG : ROUTES.REGISTER)}
            >
              <span>{isAuthenticated ? 'Go to Library' : 'Create Free Account'}</span>
              <span className={styles.ctaBtnArrow}>→</span>
            </button>
            <button
              className={styles.ctaSecondaryBtn}
              onClick={() => navigate(ROUTES.CATALOG)}
            >
              Continue as Guest
            </button>
          </motion.div>
          <motion.p className={styles.ctaNote} variants={fadeInUp}>
            ✓ No credit card required &nbsp;&nbsp; ✓ 70,000+ free books &nbsp;&nbsp; ✓ Cancel anytime
          </motion.p>
        </motion.div>
      </motion.div>
    </section>
  );
};

// ═══════════════════════════════════════════════════════════════════════════════════════════════════
// MAIN COMPONENT
// ═══════════════════════════════════════════════════════════════════════════════════════════════════

const HomePage: React.FC = () => {
  // Scroll to top on mount
  useEffect(() => {
    window.scrollTo(0, 0);
  }, []);

  return (
    <div className={styles.page}>
      <HeroSection />
      <FeaturesSection />
      <BookshelfSection />
      <GenresSection />
      <HowItWorksSection />
      <TestimonialsSection />
      <CTASection />
    </div>
  );
};

export default HomePage;
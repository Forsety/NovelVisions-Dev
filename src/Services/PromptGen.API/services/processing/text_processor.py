import re
from typing import List, Dict, Optional, Tuple
import unicodedata


class TextProcessor:
    """Service for text processing and cleaning"""
    
    def __init__(self):
        self.stop_words = set([
            "a", "an", "the", "is", "are", "was", "were", "been",
            "be", "have", "has", "had", "do", "does", "did", "will",
            "would", "could", "should", "may", "might", "must", "can",
            "shall", "to", "of", "in", "for", "on", "with", "at",
            "from", "by", "about", "as", "into", "through", "during"
        ])
    
    def clean(self, text: str) -> str:
        """Clean and normalize text"""
        
        # Remove extra whitespace
        text = " ".join(text.split())
        
        # Normalize unicode
        text = unicodedata.normalize('NFKD', text)
        
        # Remove control characters
        text = "".join(ch for ch in text if unicodedata.category(ch)[0] != "C")
        
        return text.strip()
    
    def remove_special_chars(self, text: str) -> str:
        """Remove special characters"""
        
        # Keep only alphanumeric, spaces, and basic punctuation
        return re.sub(r'[^a-zA-Z0-9\s\.\,\!\?\-]', '', text)
    
    def extract_keywords(self, text: str, limit: int = 10) -> List[str]:
        """Extract keywords from text"""
        
        # Clean text
        cleaned = self.clean(text.lower())
        
        # Split into words
        words = re.findall(r'\b[a-z]+\b', cleaned)
        
        # Remove stop words
        keywords = [w for w in words if w not in self.stop_words]
        
        # Count frequency
        word_freq = {}
        for word in keywords:
            word_freq[word] = word_freq.get(word, 0) + 1
        
        # Sort by frequency
        sorted_words = sorted(word_freq.items(), key=lambda x: x[1], reverse=True)
        
        return [word for word, _ in sorted_words[:limit]]
    
    def split_sentences(self, text: str) -> List[str]:
        """Split text into sentences"""
        
        # Simple sentence splitting
        sentences = re.split(r'[.!?]+', text)
        
        # Clean and filter
        sentences = [s.strip() for s in sentences if s.strip()]
        
        return sentences
    
    def chunk_text(self, text: str, max_length: int = 500) -> List[str]:
        """Split text into chunks"""
        
        chunks = []
        sentences = self.split_sentences(text)
        
        current_chunk = ""
        for sentence in sentences:
            if len(current_chunk) + len(sentence) + 1 <= max_length:
                if current_chunk:
                    current_chunk += " "
                current_chunk += sentence
            else:
                if current_chunk:
                    chunks.append(current_chunk)
                current_chunk = sentence
        
        if current_chunk:
            chunks.append(current_chunk)
        
        return chunks
    
    def extract_entities(self, text: str) -> Dict[str, List[str]]:
        """Extract named entities from text (simple version)"""
        
        entities = {
            "names": [],
            "places": [],
            "organizations": []
        }
        
        # Simple pattern matching for capitalized words
        words = text.split()
        
        for i, word in enumerate(words):
            if word[0].isupper() and i > 0:
                # Check if it's a name pattern
                if i > 0 and words[i-1].lower() in ["mr", "mrs", "ms", "dr", "sir"]:
                    entities["names"].append(word)
                # Check for multi-word entities
                elif i < len(words) - 1 and words[i+1][0].isupper():
                    entity = word
                    j = i + 1
                    while j < len(words) and words[j][0].isupper():
                        entity += " " + words[j]
                        j += 1
                    
                    # Heuristic classification
                    if any(suffix in entity.lower() for suffix in ["inc", "corp", "company", "ltd"]):
                        entities["organizations"].append(entity)
                    elif any(word in entity.lower() for word in ["street", "road", "city", "country"]):
                        entities["places"].append(entity)
                    else:
                        entities["names"].append(entity)
        
        # Deduplicate
        for key in entities:
            entities[key] = list(set(entities[key]))
        
        return entities

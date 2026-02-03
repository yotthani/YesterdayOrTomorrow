// Star Trek Game Sound System
window.GameSounds = (function() {
    let enabled = true;
    let masterVolume = 0.7;
    const audioContext = new (window.AudioContext || window.webkitAudioContext)();
    const sounds = {};
    
    // Generate simple synthesized sounds (no external files needed)
    function generateTone(frequency, duration, type = 'sine', attack = 0.01, decay = 0.1) {
        return () => {
            if (!enabled) return;
            
            try {
                const oscillator = audioContext.createOscillator();
                const gainNode = audioContext.createGain();
                
                oscillator.connect(gainNode);
                gainNode.connect(audioContext.destination);
                
                oscillator.type = type;
                oscillator.frequency.setValueAtTime(frequency, audioContext.currentTime);
                
                gainNode.gain.setValueAtTime(0, audioContext.currentTime);
                gainNode.gain.linearRampToValueAtTime(masterVolume, audioContext.currentTime + attack);
                gainNode.gain.linearRampToValueAtTime(0, audioContext.currentTime + duration);
                
                oscillator.start(audioContext.currentTime);
                oscillator.stop(audioContext.currentTime + duration);
            } catch (e) {
                console.log('Sound error:', e);
            }
        };
    }
    
    function generateNoise(duration, filterFreq = 1000) {
        return () => {
            if (!enabled) return;
            
            try {
                const bufferSize = audioContext.sampleRate * duration;
                const buffer = audioContext.createBuffer(1, bufferSize, audioContext.sampleRate);
                const data = buffer.getChannelData(0);
                
                for (let i = 0; i < bufferSize; i++) {
                    data[i] = Math.random() * 2 - 1;
                }
                
                const source = audioContext.createBufferSource();
                const filter = audioContext.createBiquadFilter();
                const gainNode = audioContext.createGain();
                
                source.buffer = buffer;
                filter.type = 'lowpass';
                filter.frequency.value = filterFreq;
                
                source.connect(filter);
                filter.connect(gainNode);
                gainNode.connect(audioContext.destination);
                
                gainNode.gain.setValueAtTime(masterVolume * 0.5, audioContext.currentTime);
                gainNode.gain.linearRampToValueAtTime(0, audioContext.currentTime + duration);
                
                source.start();
            } catch (e) {}
        };
    }
    
    // Define sounds
    sounds.click = generateTone(800, 0.05, 'square');
    sounds.select = generateTone(600, 0.1, 'sine');
    sounds.error = () => {
        generateTone(200, 0.15, 'sawtooth')();
        setTimeout(() => generateTone(150, 0.15, 'sawtooth')(), 100);
    };
    sounds.success = () => {
        generateTone(523, 0.1, 'sine')();
        setTimeout(() => generateTone(659, 0.1, 'sine')(), 80);
        setTimeout(() => generateTone(784, 0.15, 'sine')(), 160);
    };
    sounds.notification = generateTone(880, 0.2, 'sine');
    sounds.turn_end = () => {
        generateTone(440, 0.15, 'sine')();
        setTimeout(() => generateTone(554, 0.15, 'sine')(), 150);
        setTimeout(() => generateTone(659, 0.2, 'sine')(), 300);
    };
    
    // Combat sounds
    sounds.phaser = () => {
        const osc = audioContext.createOscillator();
        const gain = audioContext.createGain();
        osc.connect(gain);
        gain.connect(audioContext.destination);
        osc.type = 'sine';
        osc.frequency.setValueAtTime(2000, audioContext.currentTime);
        osc.frequency.exponentialRampToValueAtTime(200, audioContext.currentTime + 0.3);
        gain.gain.setValueAtTime(masterVolume * 0.3, audioContext.currentTime);
        gain.gain.exponentialRampToValueAtTime(0.01, audioContext.currentTime + 0.3);
        osc.start();
        osc.stop(audioContext.currentTime + 0.3);
    };
    
    sounds.torpedo = () => {
        const osc = audioContext.createOscillator();
        const gain = audioContext.createGain();
        osc.connect(gain);
        gain.connect(audioContext.destination);
        osc.type = 'sawtooth';
        osc.frequency.setValueAtTime(100, audioContext.currentTime);
        osc.frequency.exponentialRampToValueAtTime(50, audioContext.currentTime + 0.5);
        gain.gain.setValueAtTime(masterVolume * 0.4, audioContext.currentTime);
        gain.gain.linearRampToValueAtTime(0, audioContext.currentTime + 0.5);
        osc.start();
        osc.stop(audioContext.currentTime + 0.5);
    };
    
    sounds.explosion = generateNoise(0.4, 300);
    sounds.shield_hit = generateTone(300, 0.2, 'triangle');
    sounds.hull_hit = generateNoise(0.2, 500);
    sounds.critical = () => {
        sounds.error();
        setTimeout(() => sounds.error(), 200);
    };
    
    // Ambient sounds
    sounds.warp = () => {
        const osc = audioContext.createOscillator();
        const gain = audioContext.createGain();
        osc.connect(gain);
        gain.connect(audioContext.destination);
        osc.type = 'sine';
        osc.frequency.setValueAtTime(100, audioContext.currentTime);
        osc.frequency.exponentialRampToValueAtTime(2000, audioContext.currentTime + 0.5);
        osc.frequency.exponentialRampToValueAtTime(100, audioContext.currentTime + 1);
        gain.gain.setValueAtTime(masterVolume * 0.3, audioContext.currentTime);
        gain.gain.linearRampToValueAtTime(0, audioContext.currentTime + 1);
        osc.start();
        osc.stop(audioContext.currentTime + 1);
    };
    
    sounds.scan = () => {
        for (let i = 0; i < 3; i++) {
            setTimeout(() => generateTone(1200 + i * 200, 0.15, 'sine')(), i * 200);
        }
    };
    
    sounds.comm = generateTone(1000, 0.3, 'sine');
    sounds.alert = () => {
        const blink = () => generateTone(800, 0.1, 'square')();
        blink();
        setTimeout(blink, 200);
        setTimeout(blink, 400);
    };
    sounds.build_complete = sounds.success;
    
    return {
        play: function(soundType, volume = 1.0) {
            if (!enabled) return;
            
            // Resume audio context if suspended
            if (audioContext.state === 'suspended') {
                audioContext.resume();
            }
            
            const sound = sounds[soundType];
            if (sound) {
                const prevVolume = masterVolume;
                masterVolume = masterVolume * volume;
                sound();
                masterVolume = prevVolume;
            }
        },
        
        setEnabled: function(value) {
            enabled = value;
        },
        
        setVolume: function(value) {
            masterVolume = Math.max(0, Math.min(1, value));
        },
        
        isEnabled: function() {
            return enabled;
        }
    };
})();

console.log('🔊 Star Trek Game Sound System loaded');

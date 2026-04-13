type SoundFn = () => void;

const AudioContextCtor: typeof AudioContext =
  window.AudioContext ?? window.webkitAudioContext!;

const audioContext = new AudioContextCtor();

let enabled = true;
let masterVolume = 0.7;

function generateTone(
  frequency: number,
  duration: number,
  type: OscillatorType = 'sine',
  attack = 0.01,
): SoundFn {
  return () => {
    if (!enabled) return;
    try {
      const osc = audioContext.createOscillator();
      const gain = audioContext.createGain();
      osc.connect(gain);
      gain.connect(audioContext.destination);
      osc.type = type;
      osc.frequency.setValueAtTime(frequency, audioContext.currentTime);
      gain.gain.setValueAtTime(0, audioContext.currentTime);
      gain.gain.linearRampToValueAtTime(masterVolume, audioContext.currentTime + attack);
      gain.gain.linearRampToValueAtTime(0, audioContext.currentTime + duration);
      osc.start(audioContext.currentTime);
      osc.stop(audioContext.currentTime + duration);
    } catch (e) {
      console.log('Sound error:', e);
    }
  };
}

function generateNoise(duration: number, filterFreq = 1000): SoundFn {
  return () => {
    if (!enabled) return;
    try {
      const bufferSize = audioContext.sampleRate * duration;
      const buffer = audioContext.createBuffer(1, bufferSize, audioContext.sampleRate);
      const data = buffer.getChannelData(0);
      for (let i = 0; i < bufferSize; i++) data[i] = Math.random() * 2 - 1;
      const source = audioContext.createBufferSource();
      const filter = audioContext.createBiquadFilter();
      const gain = audioContext.createGain();
      source.buffer = buffer;
      filter.type = 'lowpass';
      filter.frequency.value = filterFreq;
      source.connect(filter);
      filter.connect(gain);
      gain.connect(audioContext.destination);
      gain.gain.setValueAtTime(masterVolume * 0.5, audioContext.currentTime);
      gain.gain.linearRampToValueAtTime(0, audioContext.currentTime + duration);
      source.start();
    } catch (_) { /* silent */ }
  };
}

const sounds: Record<string, SoundFn> = {
  click:          generateTone(800,  0.05, 'square'),
  select:         generateTone(600,  0.1,  'sine'),
  notification:   generateTone(880,  0.2,  'sine'),
  shield_hit:     generateTone(300,  0.2,  'triangle'),
  comm:           generateTone(1000, 0.3,  'sine'),
  explosion:      generateNoise(0.4, 300),
  hull_hit:       generateNoise(0.2, 500),

  error: () => {
    generateTone(200, 0.15, 'sawtooth')();
    setTimeout(() => generateTone(150, 0.15, 'sawtooth')(), 100);
  },
  success: () => {
    generateTone(523, 0.1,  'sine')();
    setTimeout(() => generateTone(659, 0.1,  'sine')(), 80);
    setTimeout(() => generateTone(784, 0.15, 'sine')(), 160);
  },
  turn_end: () => {
    generateTone(440, 0.15, 'sine')();
    setTimeout(() => generateTone(554, 0.15, 'sine')(), 150);
    setTimeout(() => generateTone(659, 0.2,  'sine')(), 300);
  },
  phaser: () => {
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
  },
  torpedo: () => {
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
  },
  warp: () => {
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
  },
  scan: () => {
    for (let i = 0; i < 3; i++) {
      setTimeout(() => generateTone(1200 + i * 200, 0.15, 'sine')(), i * 200);
    }
  },
  alert: () => {
    const blink = generateTone(800, 0.1, 'square');
    blink();
    setTimeout(blink, 200);
    setTimeout(blink, 400);
  },
  critical: () => {
    // defined after error is set, see below
  },
};

sounds.build_complete = sounds.success;
sounds.critical = () => {
  sounds.error();
  setTimeout(() => sounds.error(), 200);
};

window.GameSounds = {
  play(soundType: string, volume = 1.0): void {
    if (!enabled) return;
    if (audioContext.state === 'suspended') void audioContext.resume();
    const sound = sounds[soundType];
    if (sound) {
      const prev = masterVolume;
      masterVolume = masterVolume * volume;
      sound();
      masterVolume = prev;
    }
  },
  setEnabled(value: boolean): void {
    enabled = value;
  },
  setVolume(value: number): void {
    masterVolume = Math.max(0, Math.min(1, value));
  },
  isEnabled(): boolean {
    return enabled;
  },
};

console.log('🔊 Star Trek Game Sound System loaded');

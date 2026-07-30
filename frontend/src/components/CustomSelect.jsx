import React, { useState, useRef, useEffect } from 'react';
import { ChevronDown, Check } from 'lucide-react';

export default function CustomSelect({ options = [], value, onChange, placeholder = '' }) {
  const [isOpen, setIsOpen] = useState(false);
  const dropdownRef = useRef(null);

  useEffect(() => {
    const handleClickOutside = (event) => {
      if (dropdownRef.current && !dropdownRef.current.contains(event.target)) {
        setIsOpen(false);
      }
    };
    document.addEventListener('mousedown', handleClickOutside);
    return () => document.removeEventListener('mousedown', handleClickOutside);
  }, []);

  const selectedOption = options.find((opt) => String(opt.value) === String(value)) || options[0];

  return (
    <div className="relative w-full text-right" ref={dropdownRef}>
      <button
        type="button"
        onClick={() => setIsOpen(!isOpen)}
        className="w-full flex items-center justify-between bg-white dark:bg-[#221A32] border border-brand-peach dark:border-[#521431] text-brand-dark dark:text-[#F4F0FA] rounded-2xl px-3 py-2 text-xs sm:text-sm font-medium transition-all shadow-xs hover:border-brand-teal focus:outline-none focus:ring-2 focus:ring-brand-teal"
      >
        <span className="truncate">
          {selectedOption ? selectedOption.label : placeholder}
        </span>
        <ChevronDown className={`w-4 h-4 text-brand-teal transition-transform duration-200 shrink-0 ${isOpen ? 'rotate-180' : ''}`} />
      </button>

      {isOpen && (
        <div className="absolute z-[100] mt-1.5 w-full bg-white dark:bg-[#221A32] border border-brand-peach dark:border-[#521431] rounded-2xl shadow-xl max-h-48 overflow-y-auto p-1.5 space-y-1 animate-fade-in">
          {options.map((opt) => {
            const isSelected = String(opt.value) === String(value);
            return (
              <button
                key={opt.value}
                type="button"
                onClick={() => {
                  onChange(opt.value);
                  setIsOpen(false);
                }}
                className={`w-full flex items-center justify-between px-3 py-1.5 rounded-xl text-xs font-medium transition-all ${
                  isSelected
                    ? 'bg-brand-teal text-white font-bold'
                    : 'text-brand-dark dark:text-[#F4F0FA] hover:bg-brand-peach/40 dark:hover:bg-[#521431]/60'
                }`}
              >
                <span className="truncate">{opt.label}</span>
                {isSelected && <Check className="w-3.5 h-3.5 shrink-0" />}
              </button>
            );
          })}
        </div>
      )}
    </div>
  );
}
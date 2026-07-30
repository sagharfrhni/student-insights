import React from 'react';
import { AlertTriangle, Trash2 } from 'lucide-react';

export default function ConfirmModal({ isOpen, onClose, onConfirm, title, message, loading }) {
  if (!isOpen) return null;

  return (
    <div className="fixed inset-0 bg-brand-dark/40 backdrop-blur-xs flex items-center justify-center p-4 z-50 animate-fade-in">
      <div className="bg-brand-bg rounded-3xl p-6 w-full max-w-sm border border-brand-peach shadow-xl text-center space-y-4">
        <div className="mx-auto w-12 h-12 bg-brand-rose/20 text-brand-rose rounded-2xl flex items-center justify-center">
          <AlertTriangle className="w-6 h-6" />
        </div>
        
        <div>
          <h3 className="text-base font-bold text-brand-dark">{title || 'تأیید حذف'}</h3>
          <p className="text-xs text-brand-dark/70 mt-1 leading-relaxed">
            {message || 'آیا از حذف این مورد اطمینان دارید؟ این عملیات قابل بازگشت نیست.'}
          </p>
        </div>

        <div className="flex gap-2 justify-center pt-2">
          <button
            type="button"
            onClick={onClose}
            className="px-4 py-2 text-xs font-bold rounded-xl border border-brand-peach text-brand-dark/70 hover:bg-brand-peach/30 transition-all flex-1"
          >
            انصراف
          </button>
          <button
            type="button"
            onClick={onConfirm}
            disabled={loading}
            className="px-4 py-2 text-xs font-bold rounded-xl bg-brand-rose text-white hover:bg-brand-rose/90 transition-all flex-1 flex items-center justify-center gap-1 shadow-sm"
          >
            <Trash2 className="w-3.5 h-3.5" />
            {loading ? 'در حال حذف...' : 'حذف شود'}
          </button>
        </div>
      </div>
    </div>
  );
}
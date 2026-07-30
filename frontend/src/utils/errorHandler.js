export const translateError = (error) => {
  if (!error.response) {
    return 'خطا در ارتباط با سرور. لطفاً اتصال اینترنت خود را بررسی کنید.';
  }

  const { status, data } = error.response;

  
  if (status === 429) {
    return 'تعداد درخواست‌های شما بیش از حد مجاز است. لطفاً کمی صبر کرده و مجدداً تلاش کنید.';
  }

  
  if (status === 409) {
    return 'این رکورد هم‌زمان توسط درخواست دیگری تغییر یافته است. لطفاً صفحه را تازه‌سازی کرده و دوباره تلاش کنید.';
  }

  if (status === 400) {
    
    if (data.errors && typeof data.errors === 'object') {
      const firstKey = Object.keys(data.errors)[0];
      const firstMsg = data.errors[firstKey][0];
      if (firstMsg) return firstMsg;
    }
    
    
    if (data.detail) {
      const detail = data.detail;
      if (detail.includes('Invalid email or password')) return 'ایمیل یا رمز عبور اشتباه است.';
      if (detail.includes('already exists')) return 'این اطلاعات قبلاً در سیستم ثبت شده است.';
      if (detail.includes('Invalid refresh token')) return 'نشست شما منقضی شده است.';
      if (detail.includes('Password must be at least')) return 'رمز عبور باید حداقل ۸ کاراکتر باشد.';
      if (detail.includes('duplicates an existing exam')) return 'امتحانی با همین عنوان و تاریخ برای این درس ثبت شده است.';
      return detail;
    }
    return 'اطلاعات وارد شده معتبر نیست.';
  }

  if (status === 401) {
    return 'نشست شما به پایان رسیده است. لطفاً دوباره وارد شوید.';
  }

  if (status === 403) {
    return 'شما دسترسی لازم برای انجام این عملیات را ندارید.';
  }

  if (status === 404) {
    return 'اطلاعات مورد نظر یافت نشد.';
  }

  if (status === 500) {
    return 'خطای داخلی سرور. لطفاً بعداً تلاش کنید.';
  }

  return 'خطایی غیرمنتظره رخ داده است.';
};
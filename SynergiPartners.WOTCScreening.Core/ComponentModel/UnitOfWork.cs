using System;
using System.Collections.Generic;
using System.Text;

namespace SynergiPartners.WOTCScreening.Core.ComponentModel
{
    public abstract class UnitOfWork<T> where T : UnitOfWork<T>
    {
        public virtual bool CanExecute()
        {
            return false;
        }

        public virtual void PreExecute(Action onSuccess, Action<Exception> onFailure)
        {
            onSuccess();
        }

        public virtual void Execute(Action onSuccess, Action<Exception> onFailure)
        {
            onSuccess();
        }

        public virtual void PostExecute(Action onSuccess, Action<Exception> onFailure)
        {
            onSuccess();
        }
    }
}

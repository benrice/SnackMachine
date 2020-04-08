﻿using System;
using System.Collections.Generic;
using System.Linq;
using static SnackMachineApp.Logic.Money;

namespace SnackMachineApp.Logic
{
    public class SnackMachine: AggregateRoot
    {
        public virtual Money MoneyInside { get; protected set; }
        public virtual decimal MoneyInTransaction { get; protected set; }

        protected virtual IList<Slot> Slots { get; set; }

        public SnackMachine() {
            MoneyInside = None;
            MoneyInTransaction = 0;

            //By default, 3 slots
            Slots = new List<Slot>
            {
                new Slot(this, 1),
                new Slot(this, 2),
                new Slot(this, 3),
            };
        }

        public virtual void InsertMoney(Money money)
        {
            if (!Money.Validate(money))
                throw new InvalidOperationException();

            MoneyInTransaction += money.Amount;
            MoneyInside += money;
        }

        public virtual void ReturnMoney()
        {
            var meneyToReturn = MoneyInside.Allocate(MoneyInTransaction);
            MoneyInTransaction = 0;
            MoneyInside -= meneyToReturn;
        }

        public virtual void BuySnack(int position)
        {
            var slot = GetSlot(position);
            if (slot.SnackPile.Price > MoneyInTransaction)
                throw new InvalidOperationException();

            slot.SnackPile = slot.SnackPile.SubtaractOne();

            var change = MoneyInside.Allocate(MoneyInTransaction - slot.SnackPile.Price);
            if (change.Amount < MoneyInTransaction - slot.SnackPile.Price)
                throw new InvalidOperationException();

            MoneyInside -= change;
            MoneyInTransaction = 0;
        }

        public virtual void LoadSnacks(int position, SnackPile snackPile)
        {
            var slot = GetSlot(position);
            slot.SnackPile = snackPile;
        }

        private Slot GetSlot(int position)
        {
            return Slots.Single(x => x.Position == position);
        }

        public virtual SnackPile GetSnackPile(int position)
        {
            return GetSlot(position).SnackPile;
        }

        public virtual void LoadMoney(Money money)
        {
            MoneyInside += money;
        }
    }
}
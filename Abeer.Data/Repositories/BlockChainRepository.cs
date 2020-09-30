using Abeer.Shared;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Internal;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Abeer.Data.Repositories
{
    public class BlockChainRepository
    {
        private readonly IBlockChainDbContext _context;

        public BlockChainRepository(IBlockChainDbContext applicationDbContext)
        {
            _context = applicationDbContext;
        }

        public Task<bool> AnyAsync(BlockChainType blockChainType)
        {
            return blockChainType switch
            {
                BlockChainType.Batch => _context.BatchBlockChains.AnyAsync(),
                BlockChainType.Card => _context.CardBlockChains.AnyAsync(),
                BlockChainType.Token => _context.TokenBlockChains.AnyAsync(),
                BlockChainType.Wallet => _context.WalletBlockChains.AnyAsync(),
                _ => Task.FromResult(false),
            };
        }

        public Task<TResult> MaxAsync<TResult>(BlockChainType blockChainType, Expression<Func<BlockChain, TResult>> selector)
        {
            switch (blockChainType)
            {
                case BlockChainType.Batch:
                    return _context.BatchBlockChains.MaxAsync(selector);
                case BlockChainType.Card:
                    return _context.CardBlockChains.MaxAsync(selector);
                case BlockChainType.Token:
                    return _context.TokenBlockChains.MaxAsync(selector);
                case BlockChainType.Wallet:
                    return _context.WalletBlockChains.MaxAsync(selector);
                default:
                    throw new NotSupportedException();
            }
        }

        public Task<BlockChain> FirstOrDefaultAsync(BlockChainType blockChainType, Expression<Func<BlockChain, bool>> query, Expression<Func<BlockChain, object>> sort)
        {
            return blockChainType switch
            {
                BlockChainType.Batch => _context.BatchBlockChains.Include(b => b.Blocks).Where(query).OrderBy(sort).FirstOrDefaultAsync(),
                BlockChainType.Card => _context.CardBlockChains.Include(b => b.Blocks).Where(query).OrderBy(sort).FirstOrDefaultAsync(),
                BlockChainType.Token => _context.TokenBlockChains.Include(b => b.Blocks).Where(query).OrderBy(sort).FirstOrDefaultAsync(),
                BlockChainType.Wallet => _context.WalletBlockChains.Include(b => b.Blocks).Where(query).OrderBy(sort).FirstOrDefaultAsync(),
                _ => throw new NotSupportedException(),
            };
        }

        public async ValueTask<TokenBlockChain> AddTokenAsync(TokenBlockChain blockChain)
        {
            var entity = await _context.TokenBlockChains.AddAsync(blockChain);
            await _context.SaveChangesAsync();
            return entity.Entity;
        }

        public async ValueTask<BatchBlockChain> AddBatchAsync(BatchBlockChain blockChain)
        {
            var entity = await _context.BatchBlockChains.AddAsync(blockChain);
            await _context.SaveChangesAsync();
            return entity.Entity;
        }

        public async ValueTask<CardBlockChain> AddCardAsync(CardBlockChain blockChain)
        {
            var entity = await _context.CardBlockChains.AddAsync(blockChain);
            await _context.SaveChangesAsync();
            return entity.Entity;
        }

        public async ValueTask<Block> AddBlockAsync(Block block)
        {
            var entity = await _context.Blocks.AddAsync(block);
            await _context.SaveChangesAsync();
            return entity.Entity;
        }

        public async ValueTask<WalletBlockChain> AddWalletAsync(WalletBlockChain blockChain)
        {
            var entity = await _context.WalletBlockChains.AddAsync(blockChain);
            await _context.SaveChangesAsync();
            return entity.Entity;
        }

        public Task<List<BlockChain>> Where(BlockChainType blockChainType, Expression<Func<BlockChain, bool>> query, Expression<Func<BlockChain, object>> sort)
        {
            return blockChainType switch
            {
                BlockChainType.Batch => _context.BatchBlockChains.Include(b => b.Blocks).Where(query).OrderBy(sort).ToListAsync(),
                BlockChainType.Card => _context.CardBlockChains.Include(b => b.Blocks).Where(query).OrderBy(sort).ToListAsync(),
                BlockChainType.Token => _context.TokenBlockChains.Include(b => b.Blocks).Where(query).OrderBy(sort).ToListAsync(),
                BlockChainType.Wallet => _context.WalletBlockChains.Include(b => b.Blocks).Where(query).OrderBy(sort).ToListAsync(),
                _ => throw new NotSupportedException(),
            };
        }
    }
}
